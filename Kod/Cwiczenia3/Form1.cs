using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Media;

namespace Gra
{
    public partial class Form1 : Form
    {
        private bool p_gameOver = false;
        private int p_startTime = 0;
        private int p_currentTime = 0;
        public Game game;
        public Bitmap spiderImage, cowImage, pigImage, scareImage;
        public Sprite spiderSprite, cowSprite, pigSprite, scareSprite;
        public Bitmap grass;
        public int frameCount = 0;
        public int frameTimer = 0;
        public float frameRate = 0;
        public PointF velocity;
        public int direction = 2;
        public Bitmap planet;
        int score = 0;

        // Obsługa dżwięku
        System.Media.SoundPlayer[] audio;

        public SoundPlayer LoadSoundFile(string filename)
        {
            SoundPlayer sound = null;
            try
            {
                sound = new SoundPlayer();
                sound.SoundLocation = filename;
                sound.Load();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Errorloadingsound");
            }
            return sound;
        }

        public Form1()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed_1);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.ResumeLayout(false);

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Main();
        }

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            Shutdown();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            this.Game_KeyPressed(e.KeyCode);
        }

        public void Main()
        {
            Form form = (Form)this;
            // form musi być aktywny, aby działały klawisze
            form.Focus();
            form.Activate();
            audio = new SoundPlayer[3];

            //ładowanie dźwięku 
            audio[0] = LoadSoundFile("cow.wav");
            audio[1] = LoadSoundFile("pig.wav");

            game = new Game(ref form, 800, 600);
            //inicjacja gry
            Game_Init();
            while (!p_gameOver)
            {
               //aktualizacja timera
                p_currentTime = Environment.TickCount;
                //wywołaj Game_Update
                Game_Update(p_currentTime - p_startTime);
                //Odświeżaj 60 klatek na sekundę
                if (p_currentTime > p_startTime + 16)
                {
                    //aktualizacja timera
                    p_startTime = p_currentTime;
                    //rysuj grę 
                    Game_Draw();
                    //cykle
                    Application.DoEvents();
                    //aktualizacja obiektów gry
                    game.Update(); 
                }
                frameCount += 1;
                if (p_currentTime > frameTimer + 1000)
                {
                    frameTimer = p_currentTime;
                    frameRate = frameCount;
                    frameCount = 0;
                }
            }
            //zwolnij pamięć i wyłącz grę
            Game_End();
            Application.Exit();
        }
        public void Shutdown()
        {
            p_gameOver = true;
        }
        public bool Game_Init()
        {
            this.Text = "Pająk na polowaniu";
            grass = game.LoadBitmap("background.bmp");
            spiderImage = game.LoadBitmap("spider-sprite.bmp");
            spiderSprite = new Sprite(ref game);
            spiderSprite.Image = spiderImage;
            spiderSprite.Width = 96;// rozdzielczość pojedyńczego sprite'a
            spiderSprite.Height = 96;// 256; 
            spiderSprite.Columns = 8; //ile elementów sprite'a jest ważnych
            spiderSprite.TotalFrames = 8; //ile ramek na konkretny kierunek
            spiderSprite.AnimationRate = 25; //klatki 
            spiderSprite.X = 1; //pozycja startowa sprite'a
            spiderSprite.Y = 1;

            //krowa
            cowImage = game.LoadBitmap("cow-sprite.bmp");
            cowSprite = new Sprite(ref game);
            cowSprite.Image = cowImage;
            cowSprite.Size = new Size(96, 96);
            cowSprite.Columns = 8;
            cowSprite.TotalFrames = 8;
            cowSprite.Position = new PointF(600, 100);
            cowSprite.Velocity = new PointF(0, 1); //idzie na południe 
            cowSprite.AnimationRate = 10;

            //świnia
            pigImage = game.LoadBitmap("pig-sprite.bmp");
            pigSprite = new Sprite(ref game);
            pigSprite.Image = pigImage;
            pigSprite.Size = new Size(64, 64);
            pigSprite.Columns = 9;
            pigSprite.TotalFrames = 9;
            pigSprite.Position = new PointF(100, 450);
            pigSprite.Velocity = new PointF(2, 0); //idzie w prawo
            pigSprite.AnimationRate = 60;
            pigSprite.Direction = 2;

            //strach na wróble
            scareImage = game.LoadBitmap("scarecrow-sprite.bmp");
            scareSprite = new Sprite(ref game);
            scareSprite.Image = scareImage;
            scareSprite.Size = new Size(80, 80);
            scareSprite.Columns = 17;
            scareSprite.TotalFrames = 17;
            scareSprite.Position = new PointF(350, 220);
            scareSprite.Velocity = new PointF(0, 0); //w jednym miejscu
            scareSprite.AnimationRate = 20;
            scareSprite.Direction = -1;

            return true;
        }
        
        public void Game_Update(int time) {

            if (cowSprite.Alive)
            {
                //sprawdzenie, czy obiekty się zderzają
                if (cowSprite.IsColliding(ref spiderSprite))
                {
                    cowSprite.Alive = false;
                    score = score + 100;
                    //włącz ospowiedni dżwięk
                    audio[0].Play();
                    // *** do wykonania: prezentacja bonusu ***/
                }
            }

            if (pigSprite.Alive)
            {
                if (pigSprite.IsColliding(ref spiderSprite))
                {
                    pigSprite.Alive = false;
                    score = score + 500;
                    audio[1].Play();
                }
            }

            if (scareSprite.Alive)
            {
                if (scareSprite.IsColliding(ref spiderSprite))
                {
                    velocity = new Point(0, 1);
                    direction = 4;
                    spiderSprite.X = 1; //przeniesenie do pozycji startowej sprite'a
                    spiderSprite.Y = 1;
                    //spiderSprite.Velocity = new PointF(1, 0);
                }
            }


        }
        public void Game_Draw()
        {
            //rysuj tło
            game.DrawBitmap(ref grass, 0, 0, 800, 600);

            //poruszaj pająkiem 
            switch (direction)
            {
                case 0:
                    velocity = new Point(0, -1);
                    break;
                case 2:
                    velocity = new Point(1, 0);
                    break;
                case 4:
                    velocity = new Point(0, 1);
                    break;
                case 6:
                    velocity = new Point(-1, 0);
                    break;
            }
            spiderSprite.X += velocity.X;
            if (spiderSprite.X == 0) direction = 2;
            // doszedł do prawej krawędzi
            else if (spiderSprite.X == (this.Width - spiderSprite.Width - 6)) { direction = 6; }
            spiderSprite.Y += velocity.Y;
            if (spiderSprite.Y == 0) { direction = 4; }
            // wartość dobrana testowo !
            else if (spiderSprite.Y == (this.Height - spiderSprite.Height - 28)) { direction = 0; }
            //aanimuj i rysuj pająka
            //własny wzór bazujący na kierunkach dla pączątkowej i ostatniej ramki
            spiderSprite.Animate(direction * 4, direction * 4 + 7);
            spiderSprite.Draw();

            //zmień pozycję krowy
            cowSprite.X += cowSprite.Velocity.X;
            cowSprite.Y += cowSprite.Velocity.Y;
            //krowa porusza się w pionie tylko i dlatego sprawdzamy czy ma już zawrócić (Y)
            if (cowSprite.Y == (this.Height - cowSprite.Height - 28)) { cowSprite.Velocity = new PointF(0, -1); cowSprite.Direction = 0; }
            if (cowSprite.Y == 0) { cowSprite.Velocity = new PointF(0, 1); cowSprite.Direction = 4; }
            if (cowSprite.Alive) {
                cowSprite.Animate(cowSprite.Direction * 4, cowSprite.Direction * 4 + 7);
                cowSprite.Draw();
            }

            // świnia
            pigSprite.X += pigSprite.Velocity.X;
            pigSprite.Y += pigSprite.Velocity.Y;
            if (pigSprite.X == (550 - pigSprite.Width)) { pigSprite.Velocity = new PointF(-2, 0); pigSprite.Direction = 6; }
            if (pigSprite.X == 0) { pigSprite.Velocity = new PointF(2, 0); pigSprite.Direction = 2; }
            if (pigSprite.Alive)
            {
                pigSprite.Animate(((pigSprite.Direction * 4) + (pigSprite.Direction/2)), (pigSprite.Direction * 4 + pigSprite.Direction / 2)+8);
                pigSprite.Draw();
            }

            //strach na wróble
            if (scareSprite.Alive)
            {
                scareSprite.Animate(0,16);
                scareSprite.Draw();
            }

            // Napisz tekst na ekranie
            game.Print(0, 0, "Steruj strzałkami tam gdzie ma iść pająk");
            if (score<600) game.Print(0, 20, "Zdobyte punkty " + score.ToString());
            else game.Print(0, 20, "Zdobyte punkty " + score.ToString() + " Zjadłeś zwierzęta, koniec gry. Uruchom ponownie program.");
        }
        public void Game_End()
        {
            spiderImage = null;
            spiderSprite = null;
            grass = null;
            cowImage = null;
            cowSprite = null;
            pigImage = null;
            pigSprite = null;
            scareImage = null;
            scareImage = null;
        }
        public void Game_KeyPressed(System.Windows.Forms.Keys key)
        {
            //MessageBox.Show("key=" + key);

            switch (key)
               
            {
                case Keys.Escape:
                    Shutdown();
                    break;
                case Keys.Up:
                    direction = 0;
                    break;
                case Keys.Right:
                    direction = 2;
                    break;
                case Keys.Down:
                    direction = 4;
                    break;
                case Keys.Left:
                    direction = 6;
                    break;
                default:
                    break;
            }
        }
    }
}
