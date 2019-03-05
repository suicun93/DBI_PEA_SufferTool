﻿using DBI_ShuffleTool.Entity;
using DBI_ShuffleTool.Model;
using DBI_ShuffleTool.Utils;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using DBI_ShuffleTool.Utils.Office;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace DBI_ShuffleTool.UI
{
    public partial class ShuffleToolForm : Form
    {
        ShuffleExamModel Sem;
        List<Question> QuestionPackage = null;
        string OutPutPath;
        bool BeingDragged = false;
        int MouseDownX;
        int MouseDownY;

        public ShuffleToolForm()
        {
            InitializeComponent();

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                string inputPath = FileUtils.GetFileLocation();
                if (string.IsNullOrEmpty(inputPath)) return;
                txtLocationFolderInput.Text = inputPath;


                //Reading data
                QuestionPackage = SerializableUtils.DeserializeJson(inputPath);
                //Print result on txtLoadFileResult
                string resImported = "Questions imported: " + QuestionPackage.Count;
                int i = 0;
                foreach (var question in QuestionPackage)
                {
                    resImported = resImported + "\nQ" + (++i) + ": " + question.Candidates.Count + " candidate(s)";
                    foreach (var candidate in question.Candidates)
                    {
                        candidate.Point = question.Point;
                    }
                }
                txtLoadFileResult.Text = resImported;
                txtNumberOfTest.Maximum = TestModel.MaxNumberOfTests(QuestionPackage);
                txtNumberOfTest.Value = txtNumberOfTest.Maximum;
                btnCreateTests.Visible = true;
                btnPreview.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show(ConstantUtils.ErrorLoadFolderFailed, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void BtnCreateTests_Click(object sender, EventArgs e)
        {
            try
            {
                string location = FileUtils.SaveFileToLocation();
                if (string.IsNullOrEmpty(location))
                {
                    return;
                }
                OutPutPath = location;

                //Prepare for Test file
                Sem = new ShuffleExamModel(QuestionPackage, Convert.ToInt32(txtNumberOfTest.Value));

                //Create Test
                TestModel testModel = new TestModel();
                testModel.Path = OutPutPath;
                testModel.Sem = Sem;

                using (ProgressBarForm progress = new ProgressBarForm(testModel.CreateTests))
                {
                    progress.ShowDialog(this);
                }
                btnOpenFolder.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show(ConstantUtils.ErrorWordApp, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }



        private void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(OutPutPath + @"/DBI_Exam/");
            }
            catch (Exception)
            {
                MessageBox.Show(ConstantUtils.ErrorLoadFolderFailed, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                Thread previewCandidatePackageThread = new Thread(PreviewDocUtils.PreviewCandidatePackage);
                previewCandidatePackageThread.Start(QuestionPackage);
            }
            catch (Exception)
            {
                MessageBox.Show(ConstantUtils.ErrorWordApp, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShuffleTool_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimize_Hover(object sender, EventArgs e)
        {
            btnMinimize.Image = Properties.Resources.minimize_hover_red;
        }

        private void btnClose_Hover(object sender, EventArgs e)
        {
            btnClose.Image = Properties.Resources.close_hover_red;
        }

        private void btnClose_Leave(object sender, EventArgs e)
        {
            btnClose.Image = Properties.Resources.close;
        }

        private void btnMinimize_Leave(object sender, EventArgs e)
        {
            btnMinimize.Image = Properties.Resources.minimize;
        }

        private void controlBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                BeingDragged = true;
                MouseDownX = e.X;
                MouseDownY = e.Y;
            }
        }

        private void controlBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (BeingDragged)
            {
                Point tmpPoint = new Point(Location.X + (e.X - MouseDownX),
                    Location.Y + (e.Y - MouseDownY));
                Location = tmpPoint;
            }
        }

        private void controlBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                BeingDragged = false;
            }
        }

        private void btnPreview_MouseLeave(object sender, EventArgs e)
        {
            btnPreview.SetBounds(btnPreview.Location.X + 2, btnPreview.Location.Y + 2, 24, 24);
        }

        private void btnPreview_MouseEnter(object sender, EventArgs e)
        {
            btnPreview.SetBounds(btnPreview.Location.X - 2, btnPreview.Location.Y - 2, 28, 28);
            btnPreview.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void btnPreview_VisibleChanged(object sender, EventArgs e)
        {
            toolTipPreview.Show(ConstantUtils.TooltipPreviewAllCandidates, btnPreview, 3000);
        }

        
    }
}
