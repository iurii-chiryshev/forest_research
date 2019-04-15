using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Edu.UI;
using Edu.UI.Primitives;
using Edu.UI.Tools;
using Edu.Imaging;

namespace Edu.Forest.TrainingSet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initializeTools();
            scrollablePictureBox1.Paint += scrollablePictureBox1_Paint;
        }

        private Dictionary<ToolStripButton,Tool> _toolItems = null;
        private AboutBox1 aboutBox = new AboutBox1();
        private LinkedList<Primitive> _undoList = new LinkedList<Primitive>();
        private LinkedList<Primitive> _redoList = new LinkedList<Primitive>();

        private void initializeTools()
        {
            _toolItems = new Dictionary<ToolStripButton, Tool>();
            _toolItems.Add(handToolStripButton, new HandTool(this.scrollablePictureBox1));
            _toolItems.Add(zoomToolStripButton, new ZoomTool(this.scrollablePictureBox1));
            _toolItems.Add(cutLogToolStripButton, new CutLogTool(this.scrollablePictureBox1));
            foreach (Tool tool in _toolItems.Values.ToArray())
            {
                tool.PrimitiveDrawn += someTool_PrimitiveDrawn;
            }
        }

        private void someToolStripButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == null) return;
            if (sender is ToolStripButton == false) return;
            ToolStripButton someToolStrip = sender as ToolStripButton;
            if (someToolStrip.Checked == false) return;

            Tool someTool = null;
            try
            {
                someTool = _toolItems[someToolStrip];
            }
            catch (Exception /*ex*/)
            {
                someTool = null;
            }

            if (someTool != null)
            {
                /*откючить у остальных состояние checked*/
                foreach(ToolStripButton anyToolStrip in _toolItems.Keys.ToArray())
                {
                    if (anyToolStrip != someToolStrip)
                    {
                        anyToolStrip.Checked = false;
                    }
                }
                /*установить инструмент*/
                this.scrollablePictureBox1.Tool = someTool;
            }
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                string fileName = openFileDialog1.FileName;
                scrollablePictureBox1.Image = Image.FromFile(fileName);
                /*undo redo*/
                _undoList.Clear();
                _redoList.Clear();
                redoToolStripButton.Enabled = _redoList.Count > 0;
                undoToolStripButton.Enabled = _undoList.Count > 0;
                /*listBox*/
                writeDown("загружено изображение: " + fileName);
            }
            

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            handToolStripButton.Checked = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutBox.ShowDialog(this);
        }

        private void someTool_PrimitiveDrawn(object sender, PrimitiveEventArgs e)
        {
            
            if (e.Primitive != null)
            {
                /*добавить примитив в конец undo*/
                _undoList.AddLast(e.Primitive);
                /*очистить redo list*/
                _redoList.Clear();
                /*выставить видимость кнопок undo/redo*/
                redoToolStripButton.Enabled = _redoList.Count > 0;
                undoToolStripButton.Enabled = _undoList.Count > 0;
            }
        }

        private void scrollablePictureBox1_Paint(object sender, PaintEventArgs e)
        {
            foreach(Primitive primitive in _undoList)
            {
                primitive.Draw(e.Graphics);
            }
        }

        private void undoToolStripButton_Click(object sender, EventArgs e)
        {
            if (_undoList.Count != 0)
            {
                /*переносим последний Primitive из undo в redo*/
                _redoList.AddLast(_undoList.Last.Value);
                _undoList.RemoveLast();
            }
            redoToolStripButton.Enabled = _redoList.Count > 0;
            undoToolStripButton.Enabled = _undoList.Count > 0;
            scrollablePictureBox1.Invalidate();
        }

        private void redoToolStripButton_Click(object sender, EventArgs e)
        {
            if (_redoList.Count != 0)
            {
                /*переносим последний Primitive из redo в undo*/
                _undoList.AddLast(_redoList.Last.Value);
                _redoList.RemoveLast();
            }
            redoToolStripButton.Enabled = _redoList.Count > 0;
            undoToolStripButton.Enabled = _undoList.Count > 0;
            scrollablePictureBox1.Invalidate();
        }

        private void saveLogToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_undoList.Count <= 0) return;
            /*создать директорию для записи*/
            string path = Directory.GetCurrentDirectory() + "\\positive\\" +
                 DateTime.Now.ToString("dd\\.MM\\.yyyy\\ \\[HH\\-mm\\-ss\\.fff\\]") + "\\";
            if (createDirectory(path) == false)
            {
                MessageBox.Show("Ошибка создания директории: " + path);
                return;
            }
            /*само изображение в папку*/
            Bitmap  bitmap = scrollablePictureBox1.Image as Bitmap;
            bitmap.Save(path + "source.bmp", ImageFormat.Bmp);
            /*и сами бревана*/
            foreach (Primitive primitive in _undoList)
            {
                /*запись исходного*/
                bitmap = Tools.ROI(scrollablePictureBox1.Image as Bitmap, primitive.BoundRect);
                if (bitmap != null)
                {
                    bitmap.Save(path + Guid.NewGuid().ToString() + ".bmp", ImageFormat.Bmp);
                    writeDown("сохранена область интереса с координатами: " + primitive.BoundRect.ToString());
                }
                /**/
//                 float scale =primitive.BoundRect.Width/64.0f;
//                 for(int i = -5; i <= 5; i += 10 )
//                 {
//                     Rectangle rx = new Rectangle(primitive.BoundRect.X + (int)(i*scale),
//                                                  primitive.BoundRect.Y,
//                                                  primitive.BoundRect.Width,
//                                                  primitive.BoundRect.Height);
//                     Rectangle ry = new Rectangle(primitive.BoundRect.X,
//                                                  primitive.BoundRect.Y + (int)(i * scale),
//                                                  primitive.BoundRect.Width,
//                                                  primitive.BoundRect.Height);
//                     bitmap = Tools.ROI(scrollablePictureBox1.Image as Bitmap, rx);
//                     if (bitmap != null)
//                     {
//                         bitmap.Save(path + Guid.NewGuid().ToString() + ".bmp", ImageFormat.Bmp);
//                         writeDown("сохранена область интереса с координатами: " + rx);
//                     }
// 
//                     bitmap = Tools.ROI(scrollablePictureBox1.Image as Bitmap, ry);
//                     if (bitmap != null)
//                     {
//                         bitmap.Save(path + Guid.NewGuid().ToString() + ".bmp", ImageFormat.Bmp);
//                         writeDown("сохранена область интереса с координатами: " + ry);
//                     }
//                 }
            }
        }

        private void saveNotLogToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_undoList.Count <= 0) return;
            /*создать директорию для записи*/
            string path = Directory.GetCurrentDirectory() + "\\negative\\" +
                 DateTime.Now.ToString("dd\\.MM\\.yyyy\\ \\[HH\\-mm\\-ss\\.fff\\]") + "\\";
            if (createDirectory(path) == false)
            {
                MessageBox.Show("Ошибка создания директории: " + path);
                return;
            }
            /*само изображение в папку*/
            Bitmap bitmap = scrollablePictureBox1.Image as Bitmap;
            bitmap.Save(path + "source.bmp", ImageFormat.Bmp);
            /*и сами бревана*/
            foreach (Primitive primitive in _undoList)
            {
                bitmap = Tools.ROI(scrollablePictureBox1.Image as Bitmap, primitive.BoundRect);
                if (bitmap != null)
                {
                    bitmap.Save(path + Guid.NewGuid().ToString() + ".bmp", ImageFormat.Bmp);
                    writeDown("сохранена область интереса с координатами: " + primitive.BoundRect.ToString());
                }
            }
        }

        private bool createDirectory(string path)
        {
	        try
	        {

		        // Determine whether the directory exists.
		        if ( Directory.Exists( path ) )
		        {
			        return true;
		        }

		        DirectoryInfo di = Directory.CreateDirectory( path );
	        }
	        catch ( Exception /*e*/ ) 
	        {
		        return false;
	        }
            writeDown("создана директория: " + path);
	        return true;
        }

        private void writeDown(string str)
        {
            textBox1.AppendText(DateTime.Now.ToString("\\[dd\\.MM\\.yyyy\\THH\\:mm\\:ss\\.fff\\]") + "\t " + str + "\n");
            textBox1.ScrollToCaret();
        }

        /// <summary>
        /// Создать два файла, с именами файлов positive/negative
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void createFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo positiveDI = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\positive\\");
                DirectoryInfo negativeDI = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\negative\\");
                if (positiveDI.Exists == false || negativeDI.Exists == false)
                {
                    throw new Exception("В корневом каталоге не существует папки \\positive или \\negative");
                }
                /*обе папки есть*/
                List<string> positiveNames = new List<string>();
                List<string> negativeNames = new List<string>();
                findBmbFiles(positiveDI, positiveNames);
                findBmbFiles(negativeDI, negativeNames);

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\positive.txt",false))
                {
                    foreach (string name in positiveNames)
                    {
                         file.WriteLine(name);
                    }
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\negative.txt", false))
                {
                    foreach (string name in negativeNames)
                    {
                        file.WriteLine(name);
                    }
                }
                MessageBox.Show("Сохранено " + positiveNames.Count.ToString() + " позитивных и " +
                                negativeNames.Count.ToString() + " негативных изображения");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        /// <summary>
        /// итерационно ищет bmp файлы в указанной директории
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        private void findBmbFiles(DirectoryInfo directoryInfo, List<string> fileNames)
        {
            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo di in directories)
            {
                /*найди и сделай поиск во вложеной папке*/
                findBmbFiles(di, fileNames);
            }
            /*папок больше нет, ищи bmp*/
            FileInfo[] files = directoryInfo.GetFiles("*.bmp");
            foreach (FileInfo fi in files)
            {
                if (fi.Name == "source.bmp")
                {
                    /*это изображение не нужно*/
                    continue;
                }
                fileNames.Add(fi.FullName);
            }
        }

        private void findAndSaveCSV(string name)
        {
            // создать новый файл
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\" + name + ".csv", false))
            {
                file.Write("");
            }
            DirectoryInfo dirs = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + name + "\\");
            if (dirs.Exists == false)
            {
                throw new Exception("В корневом каталоге не существует папки c именем" + "positive");
            }
            // интересуют подиректории, которые внтутри себя содержат source.bmp
            DirectoryInfo[] includeDirs = dirs.GetDirectories();
            foreach (DirectoryInfo d in includeDirs)
            {
                FileInfo[] src = d.GetFiles("source.bmp");
                if (src != null && src.Length == 1)
                {
                    FileInfo[] files = d.GetFiles("*.bmp");
                    List<string> fileNames = new List<string>();
                    foreach (FileInfo fi in files)
                    {
                        if (fi.Name == "source.bmp")
                        {
                            /*это изображение не нужно*/
                            continue;
                        }
                        fileNames.Add(fi.FullName);
                    }
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\" + name + ".csv", true))
                    {
                        StringBuilder sb = new StringBuilder().Append(src[0].FullName).Append(",");
                        foreach (string fn in fileNames)
                        {
                            sb.Append(fn).Append(",");
                        }
                        file.WriteLine(sb.ToString());
                    }

                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                findAndSaveCSV("positive");
                findAndSaveCSV("negative");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    
}
