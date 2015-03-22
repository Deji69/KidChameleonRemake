﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Engine;
using Editor.Forms;
using System.IO;

namespace Editor.Forms
{
    partial class TilesetEditor : Form
    {
        #region Instance

        private static TilesetEditor mInstance;

        public static TilesetEditor GetOrOpen()
        {
            if (mInstance == null || mInstance.IsDisposed)
            {
                mInstance = new TilesetEditor();
                mInstance.Show();
            }

            return mInstance;
        }

        public static TilesetEditor GetInstance()
        {
            if (mInstance == null || mInstance.IsDisposed)
                return null;
         
            return mInstance;
        }

#if DESIGNMODE
        public TilesetEditor()
        {       
          InitializeComponent();
        }
#endif

        #endregion

        private TileSet mTileset;
              
        private TilesetEditor()
        {
            mTileset = new TileSet();
            InitializeComponent();

            pnlBase.SetFromTileset(TileSet.CreateBase());
            pnlBase.RefreshImage();

            pnlTileset.ImagePanel.MouseAction += new ImagePanel.MouseActionEventHandler(pnlTileset_ImageClicked);

            chkProperties.Items.Add(new PropertyBinding<TileDef> { Name = "Flags", Copy = (src, dest) => dest.SetValues(null,src.Flags,null,null)});
            chkProperties.Items.Add(new PropertyBinding<TileDef> { Name = "Sides", Copy = (src, dest) => dest.SetValues(null, null, src.Sides, null) });
            chkProperties.Items.Add(new PropertyBinding<TileDef>
            {
                Name = "Group",
                Copy = (src, dest) =>
                    {
                        dest.Groups = src.Groups;     
                    }
            });
            chkProperties.Items.Add(new PropertyBinding<TileDef> { Name = "Random Usage", Copy = (src, dest) => dest.Usage.RandomUsageWeight = src.Usage.RandomUsageWeight });

            pnlBase.SelectionMode = SelectionMode.Single;
            pnlTileset.SelectionMode = SelectionMode.Multi;

            foreach (var textbox in new TextBox[] { txtGroupTopLeft, txtGroupTopRight, txtGroupRightTop, txtGroupRightBottom, txtGroupBottomRight, txtGroupBottomLeft,
                txtGroupLeftBottom,txtGroupLeftTop})
                textbox.DoubleClick += new EventHandler(txtGroup_DoubleClick);
        }

        private class PropertyBinding<TObject>
        {
            public string Name { get; set; }

            public Action<TObject, TObject> Copy;

            public override string ToString()
            {
                return Name;
            }
        }

        void pnlTileset_ImageClicked(object sender, ImageEventArgs e)
        {                  
            if (e.Action == MouseActionType.Click)
                OnTileSelected();
        }

        private void pnlBase_Load(object sender, EventArgs e)
        {
            pnlBase.RefreshImage();
        }

        public void AddNewTiles(IEnumerable<BitmapPortion> newTiles)
        {
            int id = mTileset.GetTiles().MaxOrDefault(t => t.TileID,0);

            var editorTiles = EditorTile.Create(newTiles, id);
            mTileset.AddTiles(editorTiles.Select(p => p.TileDef));
            pnlTileset.SetFromTileset(EditorTile.CreateTileset(editorTiles));
            pnlTileset.RefreshImage();
        }
        
        public void LoadTileset()
        {
            var ts = FileDialog.ShowLoad<TileSet>(PathType.Tilesets).Data;
            if (ts == null)
                return;

            mTileset = ts;
            pnlTileset.SetFromTileset(ts);     
        }

        public void LoadTileset(TileSet ts)
        {
            mTileset = ts;
            pnlTileset.SetFromTileset(ts);
        }

        public void Save()
        {
            FileDialog.ShowSaveDialog<TileSet>(PathType.Tilesets, selectedFile =>
            {
                var bitmap = mTileset.Texture.GetImage();
                bitmap.Save(mTileset.Texture.Path.FullPath);
                BackupManager.CreateBackup(mTileset.Texture.Path.FullPath);
                return mTileset;
            });

        }

     

        private void OnTileSelected()
        {
            ApplyGroupsToCurrent();

            var selectedTile = pnlTileset.SelectedTiles().FirstOrDefault();
            if (selectedTile == null)
                return;

            var def = selectedTile.TileDef;


            tileProperties.SelectedObject = selectedTile.TileDef;
            tileProperties.Refresh();

            pnlTilePreview.Refresh();
        }

     

        private void timer1_Tick(object sender, EventArgs e)
        {
            pnlBase.RefreshImage();
            pnlTileset.RefreshImage();

            timer1.Enabled = false;
        }

        private void TilesetEditor_Resize(object sender, EventArgs e)
        {
            pnlTileset.SetFromTileset(pnlTileset.Tileset);
        }

        #region Actions

        private void btnGroup_Click(object sender, EventArgs e)
        {
            var tiles = pnlTileset.SelectedTiles().ToArray();


        }

        private void btnCopyProperties_Click(object sender, EventArgs e)
        {
            var baseTile = pnlBase.SelectedTiles().FirstOrDefault();
            if (baseTile == null)
                return;

            throw new NotImplementedException();
           // var baseEditorTile = new EditorTile(null, baseTile.TileDef);

          //  CopyProperties(baseEditorTile);
        }

        private void btnCopyFromSelected_Click(object sender, EventArgs e)
        {
            var selectedTile = tileProperties.SelectedObject as TileDef;
            if (selectedTile == null)
                return;

            CopyProperties(selectedTile);
        }


        private void CopyProperties(TileDef source)
        {
            foreach (var tile in pnlTileset.SelectedTiles())
            {
                var editorTile = mTileset.GetTiles().FirstOrDefault(p => p.TileID == tile.TileDef.TileID);
                if (editorTile == null)
                    continue;

                foreach (var item in chkProperties.CheckedItems)
                {
                    var prop = item as PropertyBinding<TileDef>;
                    prop.Copy(source, editorTile);
                }
            }
        }




        #endregion

        private void txtGroup_TextChanged(object sender, EventArgs e)
        {
            RefreshFilter();
        }

        private void chkFilterExclusive_CheckedChanged(object sender, EventArgs e)
        {
            RefreshFilter();
        }

        private void RefreshFilter()
        {
            var groups = txtGroup.Text.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

            throw new NotImplementedException();
            //if(groups.Length == 0)
            //    pnlTileset.SetTiles(mTileset.CreateTileset(this.TransparentColor, "tiles", null), TilesPerRow);
            //else if (chkFilterExclusive.Checked)
            //    pnlTileset.SetTiles(mTileset.CreateTileset(this.TransparentColor, "tiles", t => t.Usage.ContainsOnlyTheseGroups(groups)), TilesPerRow);
            //else
            //    pnlTileset.SetTiles(mTileset.CreateTileset(this.TransparentColor, "tiles", t => t.Usage.ContainsAny(groups)), TilesPerRow);
        }

        private void pnlTilePreview_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (pnlTileset.SelectedTiles().IsNullOrEmpty())
                {
                    e.Graphics.Clear(Color.Black);
                    return;
                }

                var tile = pnlTileset.SelectedTiles().FirstOrDefault().TileDef;
                if (tile == null)
                    return;

                e.Graphics.SetBlockyScaling();

                throw new NotImplementedException();
               // e.Graphics.DrawImage(tile.Image.Image, new Rectangle(0, 0, pnlTilePreview.Width, pnlTilePreview.Height), tile.Image.Region.ToSystemRec(), GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                frmLog.AddLine(ex.Message);
            }
        }

        private void tileProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Groups")
            {
                if(!e.OldValue.Equals(e.ChangedItem.Value))
                    UpdateGroupTextBoxes();
            }
        }

        private void UpdateGroupTextBoxes()
        {
            var selectedTile = pnlTileset.SelectedTiles().FirstOrDefault();
            if (selectedTile == null)
                return;

            //var usg = selectedTile.TileDef.Usage;
            //txtGroupTopLeft.Text = usg.TopLeftGroup;
            //txtGroupTopRight.Text = usg.TopRightGroup;
            //txtGroupRightTop.Text = usg.RightTopGroup;
            //txtGroupRightBottom.Text = usg.RightBottomGroup;
            //txtGroupBottomRight.Text = usg.BottomRightGroup;
            //txtGroupBottomLeft.Text = usg.BottomLeftGroup;
            //txtGroupLeftBottom.Text = usg.LeftBottomGroup;
            //txtGroupLeftTop.Text = usg.LeftTopGroup;
        }

        private void ApplyGroupsToCurrent()
        {
            //var oldSelected = tileProperties.SelectedObject as EditorTile;
            //if (oldSelected == null)
            //    return;

            //oldSelected.Groups = string.Join(" ", txtGroupTopLeft.Text, txtGroupTopRight.Text, txtGroupRightTop.Text, txtGroupRightBottom.Text, txtGroupBottomRight.Text,
            //    txtGroupBottomLeft.Text, txtGroupLeftBottom.Text, txtGroupLeftTop.Text);

        }

        private void tileProperties_SelectedObjectsChanged(object sender, EventArgs e)
        {
            UpdateGroupTextBoxes();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LoadTileset();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void AddDoubleClickGroup(TextBox box)
        {
            var items = box.Text.Split(',');
            if (!items.Contains(txtDblClickGroup.Text))
            {
                if (box.Text.IsNullOrEmpty())
                    box.Text = txtDblClickGroup.Text;
                else
                    box.Text += "," + txtDblClickGroup.Text;
            }
        }

        private void txtGroup_DoubleClick(object sender, EventArgs e)
        {
            AddDoubleClickGroup(sender as TextBox);
        }

        private void txtDblClickGroup_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtDblClickGroup_DoubleClick(object sender, EventArgs e)
        {
            AddDoubleClickGroup(txtGroupTopLeft);
            AddDoubleClickGroup(txtGroupTopRight);
            AddDoubleClickGroup(txtGroupRightTop);
            AddDoubleClickGroup(txtGroupRightBottom);
            AddDoubleClickGroup(txtGroupBottomRight);
            AddDoubleClickGroup(txtGroupBottomLeft);
            AddDoubleClickGroup(txtGroupLeftBottom);
            AddDoubleClickGroup(txtGroupLeftTop);
        }


    }
}
