﻿using NppPluginNET;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using IBMiCmd.LanguageTools;

namespace IBMiCmd.Forms
{
    public partial class rpgFileConvert : Form
    {
        public rpgFileConvert()
        {
            InitializeComponent();
        }
        private string _curFile;

        private void rpgFileConvert_Load(object sender, EventArgs e)
        {
            this._curFile = GetCurrentFileName();
            this.Text = "RPG Conversion - " + this._curFile;
            if (File.Exists(this._curFile))
            {
                string[] lines = File.ReadAllLines(this._curFile);
                //string[] newLines = new string[lines.Length];

                Boolean isPI = false, isPR = false, isDS = false;
                string curLine = "", curLineStart = "", extraLine = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    curLine = RPGFree.getFree(lines[i]);
                    
                    if (curLine.Contains(" "))
                    {
                        curLineStart = curLine.TrimStart().Split(' ')[0].ToUpper().Trim();
                    } 
                    else
                    {
                        curLineStart = "*linestart";
                    }

                    if (curLineStart != "DCL-PARM")
                    {
                        if (isPR)
                        {
                            extraLine = "".PadLeft(7) + "End-Pr;";
                            isPR = false;
                        }
                        else if (isPI)
                        {
                            extraLine = "".PadLeft(7) + "End-Pi;";
                            isPI = false;
                        }
                        else if (isDS)
                        {
                            extraLine = "".PadLeft(7) + "End-Ds;";
                            isDS = false;
                        }
                        else
                        {
                            extraLine = "";
                        }

                        if (extraLine != "")
                        {
                            AppendNewText(richTextBox2, extraLine, Color.Green);
                            AppendNewText(richTextBox2, "", Color.Black);
                        }
                    }

                    if (curLine == "*BLANK")
                    {
                        AppendNewText(richTextBox1, lines[i].TrimEnd(), Color.Red);
                    }
                    else if (curLine != "")
                    {
                        #region adding end-** blocks

                        switch (curLineStart)
                        {
                            case "DCL-PR":
                                isPR = true;
                                break;
                            case "DCL-PI":
                                isPI = true;
                                break;
                            case "DCL-DS":
                                isDS = true;
                                break;
                        }

                        #endregion

                        AppendNewText(richTextBox2, curLine, Color.Green);
                        AppendNewText(richTextBox1, lines[i].TrimEnd(), Color.Red);
                    }
                    else
                    {
                        AppendNewText(richTextBox2, lines[i].TrimEnd(), Color.Black);
                        AppendNewText(richTextBox1, lines[i].TrimEnd(), Color.Black);
                    }
                }
            } 
            else
            {
                this.Close();
                MessageBox.Show("To convert a large section of source the file must be stored locally. Save the file first and then try again.", "Information about file conversion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void AppendNewText(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text + Environment.NewLine);
            box.SelectionColor = box.ForeColor;
        }

        private void acceptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllLines(this._curFile, richTextBox2.Lines);
            RefreshWindow(this._curFile);
            this.Close();
        }

        private void declineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        public static string GetCurrentFileName()
        {
            var sb = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, sb);
            return sb.ToString();
        }

        public static void RefreshWindow(string path)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_RELOADFILE, 0, path);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MAKECURRENTBUFFERDIRTY, 0, 0);
        }
    }
}
