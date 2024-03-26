using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Pussel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        classPicbox[] picboxes = new classPicbox[0];
        Bitmap SourceBitmap;
        int[] score = new int[4];
        int currentScore;
        string[] str_score = {"", "E-0", "M-0", "H-0", "I-0" };
        bool moveBox = false;
        int[] lastGrid = new int[2];
        Point oldPos = Point.Empty;
        Image currentImage = null;
        char difficulty;

        private void Form1_Load(object sender, EventArgs e) // hämtar förra bilden och highscore
        {
            if (System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\ScoreData.dat"))
            {
                str_score = System.IO.File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + "\\ScoreData.dat");
                if (System.IO.File.Exists(str_score[0])) // om det finns en bild i stora pictureboxen
                {
                    currentImage = Image.FromFile(str_score[0]);
                    pictureBoxBig.Image = Image.FromFile(str_score[0]);
                    pictureBoxTop.Image = Image.FromFile(str_score[0]);
                    SourceBitmap = new Bitmap(pictureBoxBig.Image);
                    easyToolStripMenuItem.Visible = true;
                    mediumToolStripMenuItem.Visible = true;
                    hardToolStripMenuItem.Visible = true;
                    impossibleToolStripMenuItem.Visible = true;
                }
            }
        }
        private void grid(int gridWidth, int gridHeight) // loops through gridheight and gridwidth and makes a small picbox 
                                                         // and puts it in the bigpicturebox until the bigpicbox is filled with the small ones
        {
            Array.Resize(ref picboxes, 0);

            int Nr = 1;
            for (int i = 0; i < gridHeight; i++) 
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    PictureBox newPicBox = new PictureBox();
                    newPicBox.Size = new Size(pictureBoxBig.Width/gridWidth, pictureBoxBig.Height/gridHeight); // delar den stora picboxen i jämna delar
                    newPicBox.Location = new Point(pictureBoxBig.Width*j/gridWidth, pictureBoxBig.Height*i/gridHeight);
                    newPicBox.BorderStyle = BorderStyle.Fixed3D;
                    newPicBox.Name = "picBox" + i + j;
                    

                    Bitmap croppedImage = CropImage(SourceBitmap, new Rectangle(new Point(SourceBitmap.Width*j/gridWidth, SourceBitmap.Height*i/gridHeight), new Size(SourceBitmap.Width/gridWidth, SourceBitmap.Height/gridHeight)));
                    newPicBox.Image = croppedImage;
                    newPicBox.SizeMode = PictureBoxSizeMode.StretchImage;

                    newPicBox.MouseDown += new MouseEventHandler(PicBox_MouseDown);
                    newPicBox.MouseUp += new MouseEventHandler(PicBox_MouseUp);
                    newPicBox.MouseMove += new MouseEventHandler(PicBox_MouseMove);
                    newPicBox.MouseHover += new EventHandler(smallPicBox_MouseHover);
                    
                    Array.Resize(ref picboxes, picboxes.Length + 1);
                    picboxes[picboxes.Length - 1] = new classPicbox(Nr++, newPicBox.Location, newPicBox);
                    pictureBoxBig.Controls.Add(newPicBox);

                }
            }

            int[] Mix_Array = new int[0];
                                            // slumpar positionerna av alla picboxes
            for (int i = 0; i < picboxes.Length; i++) // loopar längden av picbox arrayen och tilldelar en slumpat tal
            {
                Random rnd = new Random();
                int Temp;
                do
                {
                    Temp = rnd.Next(1, picboxes.Length + 1);
                } while (Mix_Array.Contains(Temp)); // körs tills temp inte finns i mix_array

                Array.Resize(ref Mix_Array, Mix_Array.Length + 1);
                Mix_Array[Mix_Array.Length - 1] = Temp; // lägger temp i sista platsen i arrayen
            }
            for (int j = 0; j < picboxes.Length; j++) // loopar längden av picbox arrayen och 
            {
                Point savedPnt = picboxes[Mix_Array[j]-1].pictureBox.Location; // sparar den gamla positionen
                picboxes[Mix_Array[j]-1].pictureBox.Location = picboxes[j].pictureBox.Location; // byter plats med en annan picbox
                picboxes[j].pictureBox.Location = savedPnt;
            }

            pictureBoxBig.Image = null; // tar bort den stora bilden
        }

        public Bitmap CropImage(Bitmap source, Rectangle section) // skapar en bitmap för att dela bilden i små bitar
        {
            Bitmap bitmap = new Bitmap(section.Width, section.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
            return bitmap;
        }

        private void newPictureToolStripMenuItem_Click(object sender, EventArgs e) // hämtar bild från filer
        {
            openFileDialog1.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            openFileDialog1.Filter = "All Image Files (*.png;*.jpeg;*.gif;*.jpg;*.bmp;*.tiff;*.tif)|*.png;*.jpeg;*.gif;*.jpg;*.bmp;*.tiff;*.tif" +
            "|JPEG File Interchange Format (*.jpg *.jpeg *jfif)|*.jpg;*.jpeg;*.jfif" +
            "|BMP Windows Bitmap (*.bmp)|*.bmp";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBoxBig.Controls.Clear();
                easyToolStripMenuItem.Visible = true;
                mediumToolStripMenuItem.Visible = true;
                hardToolStripMenuItem.Visible = true;
                impossibleToolStripMenuItem.Visible = true;
                currentImage = Image.FromFile(openFileDialog1.FileName);
                pictureBoxBig.Image = Image.FromFile(openFileDialog1.FileName);
                pictureBoxTop.Image = Image.FromFile(openFileDialog1.FileName);
                str_score[0] = openFileDialog1.FileName;
                pictureBoxBig.Update();
                SourceBitmap = new Bitmap(pictureBoxBig.Image);
            }

        }

        void PicBox_MouseUp(object sender, MouseEventArgs e) // när man släpper musen
                                                             // hittar en postion åt picboxen och byter
                                                             // plats med en annan picbox
        {
            int CorrectPosCounter = 0;
            bool posFound = false;
            PictureBox pictureBox = (PictureBox)sender;
            moveBox = false;
            // loopen är detsamma som när små picboxen skapades. 
            for (int i = 0; i < lastGrid[1]; i++) // grid height
            {
                for (int j = 0; j < lastGrid[0]; j++) // gridwidth
                {
                    int x = pictureBoxBig.Width * j / lastGrid[0]; //
                    int y = pictureBoxBig.Height * i / lastGrid[1];
                    int widthSize = pictureBoxBig.Width / lastGrid[0];
                    int heightSize = pictureBoxBig.Height / lastGrid[1];
                    if (pictureBox.Location.X + widthSize/2 > x && pictureBox.Location.X + 
                        widthSize/2 < x + widthSize && pictureBox.Location.Y + heightSize/2 > y &&
                        pictureBox.Location.Y + heightSize / 2 < y + heightSize) // är musen inuti en picbox.
                                                                                 // punkterna bildar en rektangel
                                                                                 // som är lika stor som picboxen
                    {
                        
                        foreach (classPicbox p in picboxes)
                        {
                            if (p.pictureBox.Location == new Point(x, y))
                            {
                                p.pictureBox.Location = oldPos; // oldpos är positonen picboxen plockades upp från
                            }
                        }
                        pictureBox.Location = new Point(x, y);
                        posFound = true;
                        if (pictureBox.Location == oldPos) // om man inte har flyttat från dess originala position
                        {
                            posFound = false;
                        }
                    }
                }
                
            }
            if (posFound)
            {
                currentScore++;
                Scorelabel.Text = Convert.ToString(currentScore);
                // kollar om man har vunnit
                foreach (classPicbox p in picboxes)
                {
                    if (p.pictureBox.Location == p.orgPoint) // om en pixbox är på sin original position incrementeras variablen
                    {
                        CorrectPosCounter++;
                    }
                    if (CorrectPosCounter == picboxes.Length) // om variabeln är lika stor som antalet picboxes har man vunnit
                    {
                        GameWon();
                    }
                }
            }
 
        }
        void GameWon() // när man har klarat puzzlet. sparar score till highscore om den är lägre än highscore
        {
            MessageBox.Show("You Win"); // sparar highscore för varje difficulty enskild
            if (difficulty == 'e')
            {
                if (Convert.ToInt32(str_score[1].Split('-')[1]) > currentScore)
                {
                    str_score[1] = "E-" + Convert.ToString(currentScore);
                }
                Highscore.Text = Convert.ToString(str_score[1].Split('-')[1]);

            }
            else if (difficulty == 'M')
            {
                if (Convert.ToInt32(str_score[2].Split('-')[1]) > currentScore)
                {
                    str_score[1] = "M-" + Convert.ToString(currentScore);
                }
                Highscore.Text = Convert.ToString(str_score[2].Split('-')[1]);
            }
            else if (difficulty == 'h')
            {
                if (Convert.ToInt32(str_score[3].Split('-')[1]) > currentScore)
                {
                    str_score[1] = "H-" + Convert.ToString(currentScore);
                }
                Highscore.Text = Convert.ToString(str_score[3].Split('-')[1]);
            }
            else
            {
                if (Convert.ToInt32(str_score[4].Split('-')[1]) > currentScore)
                {
                    str_score[1] = "i-" + Convert.ToString(currentScore);
                }
                Highscore.Text = Convert.ToString(str_score[4].Split('-')[1]);
            }
            reset();
        }
        void PicBox_MouseDown(object sender, MouseEventArgs e) 
        {
            PictureBox pictureBox = (PictureBox)sender;
            oldPos = pictureBox.Location;
            moveBox = true;
            
        }
        void PicBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            MouseEventArgs mouse = (MouseEventArgs)e;
            if (moveBox)
            {
                // picboxen flyttas med musen
                pictureBox.Location = new Point(pictureBox.Location.X + e.X - pictureBox.Size.Width/2, pictureBox.Location.Y + e.Y - pictureBox.Size.Height / 2);
                pictureBox.BringToFront();
                
            }
        }
        private void smallPicBox_MouseHover(object sender, EventArgs e) // visar en förstorad bild på vänster
                                                                        // sida i en picbox
        {
            PictureBox picbox = sender as PictureBox;
            pictureBoxBottom.Image = picbox.Image;
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e) // sparal score och bilden när man stänger fönstret
        {
            System.IO.File.WriteAllLines(System.IO.Directory.GetCurrentDirectory() + "\\ScoreData.dat", str_score);
        }

        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();
            Highscore.Text = Convert.ToString(str_score[1].Split('-')[1]);
            difficulty = 'e';
            int width = 5;
            int height = 4;
            lastGrid[0] = width; 
            lastGrid[1] = height;
            grid(width, height); // skapar griden
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e) // medium svårighet
        {
            reset();
            Highscore.Text = Convert.ToString(str_score[2].Split('-')[1]);
            int width = 10;
            int height = 8;
            lastGrid[0] = width;
            lastGrid[1] = height;
            grid(width, height);
        }

        private void hardToolStripMenuItem_Click(object sender, EventArgs e) // svår svårighet
        {
            reset();
            Highscore.Text = Convert.ToString(str_score[3].Split('-')[1]);
            int width = 15;
            int height = 12;
            lastGrid[0] = width;
            lastGrid[1] = height;
            grid(width, height);
        }

        private void impossibleToolStripMenuItem_Click(object sender, EventArgs e) // svåraste svårigheten
        {
            reset();
            Highscore.Text = Convert.ToString(str_score[4].Split('-')[1]);
            int width = 20;
            int height = 15;
            lastGrid[0] = width;
            lastGrid[1] = height;
            grid(width, height);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();
        }
        void reset() // resetar spelet
        {
            currentScore = 0;
            pictureBoxBig.Controls.Clear();
            pictureBoxBig.Image = currentImage;
            pictureBoxTop.Image = currentImage;
            pictureBoxBottom.Image = null;
        } 
    }
}
