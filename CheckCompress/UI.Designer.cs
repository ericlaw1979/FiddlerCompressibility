namespace CheckCompress
{
    partial class CCUI
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lvQueue = new Fiddler.DoubleBufferedListView();
            this.colURL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMIME = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAsset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colServed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colGZIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colZopfli = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBrotli = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWebP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.txtInfo = new System.Windows.Forms.RichTextBox();
            this.lnkRecompute = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lvQueue
            // 
            this.lvQueue.AllowDrop = true;
            this.lvQueue.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colURL,
            this.colMIME,
            this.colAsset,
            this.colServed,
            this.colGZIP,
            this.colZopfli,
            this.colBrotli,
            this.colWebP});
            this.lvQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvQueue.EmptyText = "Drag/drop Sessions here to evaluate compressibility.";
            this.lvQueue.FullRowSelect = true;
            this.lvQueue.HideSelection = false;
            this.lvQueue.Location = new System.Drawing.Point(0, 20);
            this.lvQueue.Name = "lvQueue";
            this.lvQueue.Size = new System.Drawing.Size(609, 275);
            this.lvQueue.TabIndex = 0;
            this.lvQueue.UseCompatibleStateImageBehavior = false;
            this.lvQueue.View = System.Windows.Forms.View.Details;
            this.lvQueue.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvQueue_ColumnClick);
            this.lvQueue.SelectedIndexChanged += new System.EventHandler(this.lvQueue_SelectedIndexChanged);
            this.lvQueue.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvQueue_DragDrop);
            this.lvQueue.DragEnter += new System.Windows.Forms.DragEventHandler(this.lvQueue_DragEnter);
            this.lvQueue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvQueue_KeyDown);
            this.lvQueue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvQueue_MouseClick);
            // 
            // colURL
            // 
            this.colURL.Text = "Url";
            this.colURL.Width = 200;
            // 
            // colMIME
            // 
            this.colMIME.Text = "MIME";
            // 
            // colAsset
            // 
            this.colAsset.Text = "Bytes";
            // 
            // colServed
            // 
            this.colServed.Text = "served";
            // 
            // colGZIP
            // 
            this.colGZIP.Text = "gzip";
            // 
            // colZopfli
            // 
            this.colZopfli.Text = "zopfli";
            // 
            // colBrotli
            // 
            this.colBrotli.Text = "brotli";
            // 
            // colWebP
            // 
            this.colWebP.Text = "webp";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 292);
            this.splitter1.MinExtra = 50;
            this.splitter1.MinSize = 50;
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(609, 3);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // txtInfo
            // 
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtInfo.Location = new System.Drawing.Point(0, 295);
            this.txtInfo.Margin = new System.Windows.Forms.Padding(8);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(609, 63);
            this.txtInfo.TabIndex = 4;
            this.txtInfo.Text = "";
            // 
            // lnkRecompute
            // 
            this.lnkRecompute.Dock = System.Windows.Forms.DockStyle.Top;
            this.lnkRecompute.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkRecompute.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkRecompute.Location = new System.Drawing.Point(0, 0);
            this.lnkRecompute.Name = "lnkRecompute";
            this.lnkRecompute.Size = new System.Drawing.Size(609, 20);
            this.lnkRecompute.TabIndex = 5;
            this.lnkRecompute.TabStop = true;
            this.lnkRecompute.Text = "Recompute Compressibility...";
            this.lnkRecompute.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkRecompute.Visible = false;
            this.lnkRecompute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRecompute_LinkClicked);
            // 
            // CCUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.lvQueue);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.lnkRecompute);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.Name = "CCUI";
            this.Size = new System.Drawing.Size(609, 358);
            this.SizeChanged += new System.EventHandler(this.CCUI_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private Fiddler.DoubleBufferedListView lvQueue;
        private System.Windows.Forms.ColumnHeader colURL;
        private System.Windows.Forms.ColumnHeader colMIME;
        private System.Windows.Forms.ColumnHeader colServed;
        private System.Windows.Forms.ColumnHeader colGZIP;
        private System.Windows.Forms.ColumnHeader colZopfli;
        private System.Windows.Forms.ColumnHeader colBrotli;
        private System.Windows.Forms.ColumnHeader colAsset;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.RichTextBox txtInfo;
        private System.Windows.Forms.ColumnHeader colWebP;
        private System.Windows.Forms.LinkLabel lnkRecompute;
    }
}
