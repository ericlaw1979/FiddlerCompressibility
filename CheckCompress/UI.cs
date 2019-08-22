// TODO: Support direct drag/drop of files from disk
using Fiddler;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CheckCompress
{
    public partial class CCUI : UserControl
    {
        // This extension installs this one.
        static readonly string sCWebPPath = CONFIG.GetPath("Scripts") + "tools\\cwebp.exe";

        // Fiddler should include all of these.
        static readonly string sZopfliPath = CONFIG.GetPath("Tools") + "zopfli.exe";
        static readonly string sBrotliPath = CONFIG.GetPath("Tools") + "brotli.exe";
        static readonly string sPNGDistillPath = CONFIG.GetPath("Tools") + "pngdistill.exe";

        bool bSuppressUpdates = false;

        /// <summary>
        /// What's the maximum size of a raw file that we'll compress with Zopfli?
        /// </summary>
        uint MAX_ZOPFLI_SIZE = 1 << 22;

        public CCUI()
        {
            InitializeComponent();
            if (CONFIG.flFontSize != Font.Size)
            {
                Font = new Font(Font.FontFamily, CONFIG.flFontSize);
            }
            lvQueue.ListViewItemSorter = new ListViewItemComparer();
            FiddlerApplication.Prefs.AddWatcher("fiddler.ui.font.size", OnPrefChange);

            MAX_ZOPFLI_SIZE = (uint)FiddlerApplication.Prefs.GetInt32Pref("extensions.compressibility.Zopfli.MaxSize", (int)MAX_ZOPFLI_SIZE);

            foreach (string sPath in new[]{sCWebPPath, sZopfliPath, sBrotliPath, sPNGDistillPath})
            {
                if (!File.Exists(sPath))
                {
                    string sErr = String.Format("missing a required tool: {0}", sPath);
                    FiddlerApplication.Log.LogString("!Compressibility extension is " + sErr);
                    this.txtInfo.Text = sErr;
                }
            }
        }

        private void OnPrefChange(object sender, PrefChangeEventArgs oPCE)
        {
            if (oPCE.PrefName == "fiddler.ui.font.size")
            {
               Font = new Font(Font.FontFamily, CONFIG.flFontSize);
            }
        }

        private void AddSessionsToUI(Session[] oSessions)
        {
            int iCount = 0;
            foreach (Session oSession in oSessions)
            {
                try
                {
                    if (!oSession.bHasResponse || oSession.HTTPMethodIs("CONNECT")
                        || (oSession.responseBodyBytes.Length < 1)) continue;

                    Asset thisAsset = new Asset();
                    thisAsset.oS = oSession;

                    iCount++;

                    HTTPResponseHeaders rhHeaders = oSession.ResponseHeaders;

                    thisAsset.arrRaw = Utilities.Dupe(oSession.responseBodyBytes);
                    if (oSession.ResponseHeaders.ExistsAndContains("Transfer-Encoding", "chunked"))
                    {
                        rhHeaders = (HTTPResponseHeaders)rhHeaders.Clone();
                        rhHeaders.Remove("Transfer-Encoding");
                        thisAsset.arrRaw = Utilities.doUnchunk(thisAsset.arrRaw);
                    }
                    thisAsset.cbServed = (uint)thisAsset.arrRaw.Length;

                    Utilities.utilDecodeHTTPBody(rhHeaders, ref thisAsset.arrRaw);
                    thisAsset.cbRaw = (uint)thisAsset.arrRaw.Length;

                    ListViewItem oLVI = new ListViewItem(new[] {
                                                            oSession.fullUrl,
                                                            Utilities.TrimBefore(oSession.oResponse.MIMEType, '/'),
                                                            thisAsset.cbRaw.ToString("N0"),
                                                            thisAsset.cbServed.ToString("N0"),
                                                            "-",
                                                            "-",
                                                            "-",
                                                            "-"
                                                            }
                                                                );

                    if (thisAsset.cbServed < 1)
                    {
                        oLVI.ForeColor = Color.Gray;
                        oLVI.Font = new Font(oLVI.Font, FontStyle.Strikeout);
                    }

                    thisAsset.sMIME = oSession.oResponse.MIMEType.ToLower();

                    if (thisAsset.sMIME.OICStartsWithAny("image/", "video/", "font/"))
                    {
                        oLVI.ForeColor = Color.Gray;
                    }
                    thisAsset.oLVI = oLVI;
                    oLVI.Tag = thisAsset;
                    lvQueue.Items.Add(oLVI);
                }
                catch (Exception eX)
                {
                    MessageBox.Show(eX.Message + "\nSession #" + oSession.id.ToString() + ": " + oSession.fullUrl, "Failed Analysis");
                }
            }

            if (iCount > 0)
            {
                lnkRecompute.Visible = true;
            }
        }

        private void lvQueue_DragDrop(object sender, DragEventArgs e)
        {
            Session[] oSessions = (Session[])e.Data.GetData("Fiddler.Session[]");
            if ((oSessions == null) || (oSessions.Length < 1)) {
                Debug.Assert(false, "Unexpected drop type.");
                return;
                /*  TODO: Support files from disk
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] arrDroppedFiles = (string[])e.Data.GetData("FileDrop", false);
                    foreach (string sFilename in arrDroppedFiles)
                    {
                        if (sFilename.EndsWith(".saz", StringComparison.OrdinalIgnoreCase))
                        {
                            oDroppedSessions = Utilities.ReadSessionArchive(sFilename, true);
                        }
                    }
                    e.Effect = DragDropEffects.Copy;
                }*/
            }
            AddSessionsToUI(oSessions);
        }

        /// <summary>
        /// Use Google's ZopFli compressor for compression instead of the default compressor
        /// </summary>
        private static byte[] ZopFliCompress(byte[] arrIn, out uint iMS)
        {
            string sTempFile = CONFIG.GetPath("Root") + "tmpToCompress" + Guid.NewGuid();
            string sOutFile = String.Concat(sTempFile, ".gz");

            File.WriteAllBytes(sTempFile, arrIn);
            string sParams = String.Format("--gzip {0} \"{1}\"",
                                           FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.Zopfli.Args", String.Empty),
                                           sTempFile);

            int iExitCode = 0;
            Stopwatch oSW = Stopwatch.StartNew();
            Utilities.GetExecutableOutput(sZopfliPath, sParams, out iExitCode);
            iMS = (uint)oSW.ElapsedMilliseconds;

            if (0 != iExitCode)
            {
                throw new Exception("ZopFli conversion failed");
            }

            byte[] arrOut = File.ReadAllBytes(sOutFile);

            #region DeleteTempFiles
            try
            {
                File.Delete(sTempFile);
                FiddlerApplication.LogLeakedFile(sTempFile);
            }
            catch
            {
                Debug.Assert(false, "Could not delete Temp Input file");
            }

            try
            {
                File.Delete(sOutFile);
            }
            catch
            {
                FiddlerApplication.LogLeakedFile(sOutFile);
                Debug.Assert(false, "Could not delete Temp Output file");
            }
            #endregion DeleteTempFiles
            return arrOut;
        }

        private static byte[] BrotliCompress(byte[] arrIn, out uint iMS)
        {
            string sTempFile = CONFIG.GetPath("Root") + "tmpToCompress" + Guid.NewGuid();
            string sOutFile = String.Concat(sTempFile, ".br");

            File.WriteAllBytes(sTempFile, arrIn);
            string sParams = String.Format("--in \"{0}\" --out \"{1}\" {2}",
                                           sTempFile,
                                           sOutFile,
                                           FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.Brotli.Args", String.Empty));

            int iExitCode = 0;
            Stopwatch oSW = Stopwatch.StartNew();
            Utilities.GetExecutableOutput(sBrotliPath, sParams, out iExitCode);
            iMS = (uint)oSW.ElapsedMilliseconds;
            if (0 != iExitCode)
            {
                throw new Exception("Brotli conversion failed");
            }

            byte[] arrOut = File.ReadAllBytes(sOutFile);

            #region DeleteTempFiles
            try
            {
                File.Delete(sTempFile);
                FiddlerApplication.LogLeakedFile(sTempFile);
            }
            catch
            {
                Debug.Assert(false, "Could not delete Temp Input file");
            }

            try
            {
                File.Delete(sOutFile);
            }
            catch
            {
                FiddlerApplication.LogLeakedFile(sOutFile);
                Debug.Assert(false, "Could not delete Temp Output file");
            }
            #endregion DeleteTempFiles
            return arrOut;
        }

        private static byte[] PNGDistill(byte[] arrIn)
        {
            string sTempFile = CONFIG.GetPath("Root") + "tmpToCompress" + Guid.NewGuid();
            File.WriteAllBytes(sTempFile, arrIn);

            int iResult;

            Utilities.GetExecutableOutput(sPNGDistillPath, "\"" + sTempFile + "\"" + " REPLACE", out iResult);

            byte[] arrNew = File.ReadAllBytes(sTempFile);

            #region DeleteTempFile
            try
            {
                File.Delete(sTempFile);
                FiddlerApplication.LogLeakedFile(sTempFile);
            }
            catch
            {
                Debug.Assert(false, "Could not delete Temp Input file");
            }
            #endregion DeleteTempFile
            return arrNew;
        }

        private static byte[] ToWebP(bool bLossless, byte[] arrIn, out uint iMS)
        {
            string sTempFile = CONFIG.GetPath("Root") + "tmpToCompress" + Guid.NewGuid();
            string sOutFile = CONFIG.GetPath("Root") + Guid.NewGuid() + ".webp";
            File.WriteAllBytes(sTempFile, arrIn);

            int iResult;

            string sArgs = bLossless ?
                FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.WebPLossless.Args", "-m 6") :
                FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.WebPLossy.Args", "-m 6");

            Stopwatch oSW = Stopwatch.StartNew();
            string s = Utilities.GetExecutableOutput(sCWebPPath, ((bLossless) ? "-lossless " : "") +
                " " + sArgs + " \"" + sTempFile + "\" -o \"" + sOutFile + "\"", out iResult);
            iMS = (uint)oSW.ElapsedMilliseconds;

            byte[] arrNew = File.ReadAllBytes(sOutFile);

            #region DeleteTempFiles
            try
            {
                File.Delete(sTempFile);
                FiddlerApplication.LogLeakedFile(sTempFile);
            }
            catch
            {
                Debug.Assert(false, "Could not delete Temp Input file");
            }
            try
            {
                File.Delete(sOutFile);
            }
            catch
            {
                FiddlerApplication.LogLeakedFile(sOutFile);
                Debug.Assert(false, "Could not delete Temp Output file");
            }
            #endregion DeleteTempFiles
            return arrNew;
        }


        private void PerformCompression()
        {
            FiddlerApplication.UI.SetStatusText("Updating Compressibility information...");

            if (lvQueue.UseWaitCursor) return;

            lnkRecompute.Visible = false;
            BackgroundWorker oBW = new BackgroundWorker();

            lvQueue.UseWaitCursor = true;

            oBW.DoWork += delegate (object s, DoWorkEventArgs ea)
            {
                try
                {
                    foreach (ListViewItem oLVI in lvQueue.Items)
                    {
                        try
                        {
                            Asset a = oLVI.Tag as Asset;
                            if (null == a)
                            {
                                Debug.Assert(false);
                                continue;
                            }

                            if ((a.psState == ProcessingState.NotStarted) && (a.cbRaw > 0))
                            {
                                a.psState = ProcessingState.Running;

                                Stopwatch oSW;

                                if ((a.sMIME.StartsWith("audio/") ||
                                    a.sMIME.StartsWith("video/")) &&
                                    FiddlerApplication.Prefs.GetBoolPref("extensions.compressibility.SkipAudioVideo", true))
                                {
                                    a.psState = ProcessingState.Completed;
                                    a.oLVI.SubItems[4].Text = "skipped";
                                    a.oLVI.SubItems[5].Text = "skipped";
                                    a.oLVI.SubItems[6].Text = "skipped";
                                    a.oLVI.SubItems[7].Text = "skipped";
                                    continue;
                                }

                                switch (a.sMIME)
                                {
                                    case "image/png":

                                        oSW = Stopwatch.StartNew();
                                        if (a.cbRaw < MAX_ZOPFLI_SIZE)
                                        {
                                            byte[] arrDistilled = PNGDistill(a.arrRaw);
                                            a.msZopfli = (uint)oSW.ElapsedMilliseconds;
                                            a.cbZopfli = (uint)arrDistilled.Length;
                                        }

                                        byte[] arrWebPL = ToWebP(true, a.arrRaw, out a.msWebP);
                                        a.cbWebP = (uint)arrWebPL.Length;

                                        a.oLVI.SubItems[4].Text = "n/a";
                                        a.oLVI.SubItems[5].Text = (a.cbZopfli > 0) ? a.cbZopfli.ToString("N0") : "skipped";
                                        a.oLVI.SubItems[6].Text = "n/a";
                                        a.oLVI.SubItems[7].Text = a.cbWebP.ToString("N0");

                                        break;
                                    case "image/jpeg":
                                    case "image/jpg":
                                        oSW = Stopwatch.StartNew();
                                        byte[] arrGZ = Utilities.GzipCompress(a.arrRaw);
                                        a.msGZip = (uint)oSW.ElapsedMilliseconds;
                                        a.cbGZip = (uint)arrGZ.Length;

                                        byte[] arrWebP = ToWebP(false, a.arrRaw, out a.msWebP);
                                        a.cbWebP = (uint)arrWebP.Length;

                                        a.oLVI.SubItems[4].Text = a.cbGZip.ToString("N0");
                                        a.oLVI.SubItems[5].Text = "n/a";
                                        a.oLVI.SubItems[6].Text = "n/a";
                                        a.oLVI.SubItems[7].Text = a.cbWebP.ToString("N0");
                                        break;

                                    default:
                                        oSW = Stopwatch.StartNew();
                                        byte[] arrjGZ = Utilities.GzipCompress(a.arrRaw);
                                        a.msGZip = (uint)oSW.ElapsedMilliseconds;
                                        a.cbGZip = (uint)arrjGZ.Length;
                                        
                                        byte[] arrBrotli = BrotliCompress(a.arrRaw, out a.msBrotli);
                                        a.cbBrotli = (uint)arrBrotli.Length;

                                        if (a.cbRaw < MAX_ZOPFLI_SIZE)
                                        {
                                            byte[] arrZopfli = ZopFliCompress(a.arrRaw, out a.msZopfli);
                                            a.cbZopfli = (uint)arrZopfli.Length;
                                        }

                                        // TODO: Following UI Update lines should be running on
                                        // the UI thread...
                                        a.oLVI.SubItems[4].Text = a.cbGZip.ToString("N0");
                                        a.oLVI.SubItems[5].Text = (a.cbZopfli > 0) ? a.cbZopfli.ToString("N0") : "skipped";
                                        a.oLVI.SubItems[6].Text = a.cbBrotli.ToString("N0");
                                        a.oLVI.SubItems[7].Text = "n/a";        // WebP

                                        float flDelta = 100 - (100 * a.cbBrotli / (float)a.cbZopfli);
                                        if ((flDelta > 15) && (a.cbBrotli + 1500 < a.cbZopfli))
                                        {
                                            oLVI.BackColor = Color.GreenYellow;
                                        }
                                        else
                                            if (flDelta < 4)
                                        {
                                            oLVI.BackColor = Color.LightCoral;
                                        }
                                        break;
                                }
                            }

                            a.psState = ProcessingState.Completed;
                        }
                        catch { }
                    }
                }
                finally
                {
            
                }
            };

            oBW.RunWorkerCompleted += delegate {
                FiddlerApplication.UI.SetStatusText("Updated Compressibility information.");
                lvQueue.UseWaitCursor = false;
                RefreshInfo();
            };

            oBW.RunWorkerAsync();
        }

        private void lvQueue_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = (e.Data.GetDataPresent("Fiddler.Session[]") 
                     // TODO: || e.Data.GetDataPresent(DataFormats.FileDrop)) 
                       ) ? DragDropEffects.Copy : e.Effect = DragDropEffects.None;
        }

        private void lvQueue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = e.SuppressKeyPress = true;
                lvQueue.BeginUpdate();
                foreach (ListViewItem oLVI in lvQueue.SelectedItems)
                {
                    lvQueue.Items.Remove(oLVI);
                }
                if (lvQueue.FocusedItem != null)
                {
                    lvQueue.FocusedItem.Selected = true;
                }
                lvQueue.EndUpdate();
                RefreshInfo();
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                PerformCompression();
                e.Handled = e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyData == (Keys.Control | Keys.A))
            {
                try
                {
                    bSuppressUpdates = true;
                    lvQueue.BeginUpdate();
                    foreach (ListViewItem lvi in lvQueue.Items)
                    {
                        lvi.Selected = true;
                    }
                }
                finally
                {
                    lvQueue.EndUpdate();
                    bSuppressUpdates = false;
                    RefreshInfo();
                }
                e.Handled = e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyData == (Keys.C | Keys.Control))
            {
                StringBuilder sbToCopy = new StringBuilder();
                
                foreach (ColumnHeader ch in lvQueue.Columns)
                {
                    sbToCopy.AppendFormat("{0}\t", ch.Text);
                }
                sbToCopy.AppendLine();

                foreach (ListViewItem oLVI in lvQueue.SelectedItems)
                {
                    foreach (ListViewItem.ListViewSubItem siLVI in oLVI.SubItems)
                    {
                        var sText = siLVI.Text;
                        if ((sText == "-") || (sText == "n/a") || (sText == "skipped")) sText = String.Empty;
                        sbToCopy.AppendFormat("{0}\t", sText);
                    }
                    sbToCopy.AppendLine();

                }
                Clipboard.SetText(sbToCopy.ToString());

                e.Handled = e.SuppressKeyPress = true;
                return;
            }
        }

        private void lvQueue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bSuppressUpdates) return;
            RefreshInfo();
        }

        private void RefreshInfo()
        {
            Trace.WriteLine(Environment.TickCount.ToString() + "\tRefresh Info: " + lvQueue.SelectedItems.Count.ToString());
            if (lvQueue.Items.Count < 1)
            {
                txtInfo.Text = "Drag and drop Sessions to the list above to evaluate compression effectiveness.";
                return;
            }
            if (lvQueue.SelectedItems.Count == 1)
            {
                ListViewItem oLVI = lvQueue.SelectedItems[0];
                Asset a = oLVI.Tag as Asset;
                if (null == a)
                {
                    Debug.Assert(false);
                    txtInfo.Text = "";
                    return;
                }

                switch (a.psState)
                {
                    case ProcessingState.NotStarted:
                        txtInfo.Text = "Compressibility of this resource has not been calculated.";
                        break;

                    case ProcessingState.Running:
                        txtInfo.Text = "Compressibility of this resource is being calculated.";
                        break;

                    case ProcessingState.Completed:
                        txtInfo.Text = String.Format(
                            "Compression of this resource:\nserver:\t{8:N1}%\ngzip:\t{0:N1}%\t(took {1:N0}ms)\nzopfli:\t{2:N1}%\t(took {3:N0}ms)\t{4:N1}% smaller than basic gzip\nbrotli:\t{5:N1}%\t(took {6:N0}ms)\t{7:N1}% smaller than zopfli\n",
                            100 - (100 * a.cbGZip / (float)a.cbRaw),
                            a.msGZip,
                            100 - (100 * a.cbZopfli / (float)a.cbRaw),
                            a.msZopfli,
                            100 - (100 * a.cbZopfli / (float)a.cbGZip),
                            100 - (100 * a.cbBrotli / (float)a.cbRaw),
                            a.msBrotli,
                            100 - (100 * a.cbBrotli / (float)a.cbZopfli),
                            100 - (100 * a.cbServed / (float)a.cbRaw)
                            );
                        break;
                }
                return;
            }

            if (lvQueue.SelectedItems.Count < 1)
            {
                txtInfo.Text = "Select one or more Sessions to see compression savings.";
                return;
            }

            // Iterate ALL Selected and sum their data.
            int iBrotliSavings = 0;
            int iZopfliSavings = 0;
            int iWebPSavings = 0;
            int iTotalBytesServed = 0;
            int iTotalBytesRaw = 0;
            int iCount = 0;
            int iNotProcessed = 0;

            foreach (ListViewItem oLVI in lvQueue.SelectedItems)
            {
                Asset thisAsset = oLVI.Tag as Asset;
                if ((null == thisAsset) || (thisAsset.psState != ProcessingState.Completed))
                {
                    iNotProcessed++;
                    continue;
                }

                iCount++;

                iTotalBytesServed += (int)thisAsset.cbServed;
                iTotalBytesRaw += (int)thisAsset.cbRaw;

                if (thisAsset.cbBrotli > 0)
                {
                    int iB = (int)(thisAsset.cbServed - thisAsset.cbBrotli);
                    if (iB > 0) iBrotliSavings += iB;
                }

                if (thisAsset.cbZopfli > 0)
                {
                    int iZ = (int)(thisAsset.cbServed - thisAsset.cbZopfli);
                    if (iZ > 0) iZopfliSavings += iZ;
                }

                if (thisAsset.cbWebP > 0)
                {
                    int iW = (int)(thisAsset.cbServed - thisAsset.cbWebP);
                    if (iW > 0) iWebPSavings += iW;
                }
            }

            string sNotProcessed = (iNotProcessed > 0) ? String.Format("{0} resources have not been processed yet!\n", iNotProcessed) : String.Empty;

            if (iCount < 1)
            {
                txtInfo.Text = sNotProcessed;
            }
            else
            {
                txtInfo.Text = String.Format(
                             "{8}These {0:N0} resources accounted for {1:N0} bytes served; " +
                             "server compression saved {9:N0} bytes.\n" +
                             "\tUsing Zopfli would save\t{2:N0} more bytes\t({3:N1}%).\n" +
                             "\tUsing Brotli would save\t{4:N0} more bytes\t({5:N1}%).\n" +
                             "\tUsing WebP would save\t{6:N0} more bytes\t({7:N1}%).\n",

                             iCount,
                             iTotalBytesServed,
                             iZopfliSavings,
                             (100 * iZopfliSavings / (float)iTotalBytesServed),
                             iBrotliSavings,
                             (100 * iBrotliSavings / (float)iTotalBytesServed),
                             iWebPSavings,
                             (100 * iWebPSavings / (float)iTotalBytesServed),
                             sNotProcessed,
                             iTotalBytesRaw - iTotalBytesServed
                             );
            }
        }

        private void lvQueue_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparer)lvQueue.ListViewItemSorter).Column = e.Column;
            lvQueue.BeginUpdate();
            lvQueue.Sort();
            if (lvQueue.SelectedIndices.Count > 0)
            {
                lvQueue.EnsureVisible(lvQueue.SelectedIndices[0]);
            }
            lvQueue.EndUpdate();
        }

        public int GetSubItemIndexFromPoint(Point ptClient)
        {
            return lvQueue.HitTest(ptClient).Item.SubItems.IndexOf(lvQueue.HitTest(ptClient).SubItem);
        }

        private void lvQueue_MouseClick(object sender, MouseEventArgs e)
        {
            // Trace.WriteLine(Environment.TickCount.ToString() + " - Enter MouseClick!");
            if ((MouseButtons.Left == e.Button) && (Keys.Alt == (Control.ModifierKeys & Keys.Alt)))
            {
                // Trace.WriteLine(Environment.TickCount.ToString() + " - Was ALT+MouseClick!");
                try
                {
                    bSuppressUpdates = true;
                    lvQueue.BeginUpdate();

                    ListViewItem oLVI = lvQueue.GetItemAt(e.Location.X, e.Location.Y);
                    int iX = GetSubItemIndexFromPoint(e.Location);
                    if ((iX >= 0) && (oLVI.SubItems.Count >= iX))
                    {

                        string sSearchVal = oLVI.SubItems[iX].Text;

                        // Shall we replace the selected set (instead of just adding to it?)
                        bool bReplaceSelection = (Keys.Control != (Control.ModifierKeys & Keys.Control));

                        if (bReplaceSelection) lvQueue.SelectedItems.Clear();

                        foreach (ListViewItem LVI in lvQueue.Items)
                        {
                            if (LVI.SubItems[iX].Text == sSearchVal) LVI.Selected = true;
                        }
                    }
                }
                finally
                {
                    lvQueue.EndUpdate();
                    bSuppressUpdates = false;
                    RefreshInfo();
                }
            }
            // Trace.WriteLine(Environment.TickCount.ToString() + " - Exit MouseClick!");
        }

        private void lnkRecompute_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PerformCompression();
        }

        private void CCUI_SizeChanged(object sender, EventArgs e)
        {
            if (lvQueue.Height < 50)
            {
                txtInfo.Height = 50;
            }
        }
    }

    class ListViewItemComparer : IComparer
    {
        private int col;
        internal bool ascending;

        public ListViewItemComparer()
        {
            col = 0;                                                    // Do numeric sort on ID by default
            ascending = true;
        }

        public int Column
        {
            get
            {
                return col;
            }
            set
            {
                if (value == col) ascending = !ascending;
                col = value;
            }
        }

        /// <summary>
        /// Compares two listview items, sorting based on the active "sortcolumn".
        /// Note: This is an extremely perf-critical function.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            int result = -1;
            ListViewItem lviX = (ListViewItem)x;
            ListViewItem lviY = (ListViewItem)y;

            if (lviX.SubItems.Count <= col)
                result = -1;
            else if (lviY.SubItems.Count <= col)
                result = 1;
            else
            {
                if (col > 1)
                {
                    var sX = lviX.SubItems[col].Text;
                    var sY = lviY.SubItems[col].Text;
                    if ((sX == "-") || (sX == "n/a") || (sX == "skipped")) sX = "-1";
                    if ((sY == "-") || (sY == "n/a") || (sY == "skipped")) sY = "-1";
                    result = int.Parse(sX, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign)
                    .CompareTo(int.Parse(sY, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign));
                }
                else
                {
                    result = String.Compare(lviX.SubItems[col].Text, lviY.SubItems[col].Text, StringComparison.Ordinal);
                }
            }

            if (!ascending)
            {
                result = (0 - result);
            }
            return result;
        }
    }

    enum ProcessingState : byte
    {
        NotStarted = 0,
        Running = 1,
        Completed = 2
    }

    class Asset
    {
        public ListViewItem oLVI;
        public Session oS;
        public string sMIME;
        public byte[] arrRaw;
        public uint cbRaw;
        public uint cbServed;
        public uint cbGZip;
        public uint msGZip;
        public uint cbZopfli;
        public uint msZopfli;
        public uint cbBrotli;
        public uint msBrotli;

        public uint cbWebP;
        public uint msWebP;

        public ProcessingState psState;
    }
}
