using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace billiards_bitmap
{
    public partial class GameFrame : Form
    {
        public static int scale = 2;
        public static List<Ball> balls = new List<Ball>();
        public static List<FallingEffect> falling = new List<FallingEffect>();
        public static List<Line> bags = new List<Line>();
        public static Point last_click_position;
        public static string page = "initial";
        public static int total_pools;
        public static double strength;
        public static bool new_record = false;
        private static Point trajs, traje;
        private static System.Windows.Forms.ProgressBar strength_bar;
        private static double draw_direction;
        Bitmap stick = Helper.LoadBmp("Stick");
      
        Bitmap table = Helper.LoadBmp("TableBlue");
        Bitmap wallpaper = Helper.LoadBmp("WallPaper");
        Bitmap win_background = Helper.LoadBmp("Win");
        Bitmap lose_background = Helper.LoadBmp("Lose");

        Bitmap name = Helper.LoadBmp("Name");
        Bitmap start_button = Helper.LoadBmp("StartGame");

        public GameFrame()
        {
            InitializeComponent();

            this.BackgroundImage = Helper.LoadBmp("BackGround"); // 背景图
          

            //
            // strength_bar
            //
            strength_bar = new System.Windows.Forms.ProgressBar();
            strength_bar.Location = new System.Drawing.Point(600, 30);
            strength_bar.Name = "strength_bar";
            strength_bar.Size = new System.Drawing.Size(321, 23);
            strength_bar.TabIndex = 0;
            strength_bar.Visible = false;
            Controls.Add(strength_bar);

            name.MakeTransparent(Color.Transparent);
            start_button.MakeTransparent(Color.Transparent);

            //static Point tab_ll = new Point(255, 155);
            //static Point tab_ur = new Point(1330, 700);
            bags.Add(new Line(new Point(Ball.tab_ll.X - 5, Ball.tab_ll.Y), new Point(Ball.tab_ll.X - 5, Ball.tab_ll.Y + 30))); //左
            bags.Add(new Line(new Point(Ball.tab_ll.X - 5, Ball.tab_ur.Y - 30), new Point(Ball.tab_ll.X - 5, Ball.tab_ur.Y)));

            bags.Add(new Line(new Point(Ball.tab_ll.X - 5, Ball.tab_ur.Y), new Point(Ball.tab_ll.X + 30, Ball.tab_ur.Y))); //下
            bags.Add(new Line(new Point(Ball.tab_ll.X + 525, Ball.tab_ur.Y), new Point(Ball.tab_ll.X + 575, Ball.tab_ur.Y)));
            bags.Add(new Line(new Point(Ball.tab_ur.X - 30, Ball.tab_ur.Y), new Point(Ball.tab_ur.X, Ball.tab_ur.Y)));

            bags.Add(new Line(new Point(Ball.tab_ur.X, Ball.tab_ur.Y - 30), new Point(Ball.tab_ur.X, Ball.tab_ur.Y)));
            bags.Add(new Line(new Point(Ball.tab_ur.X, Ball.tab_ll.Y), new Point(Ball.tab_ur.X, Ball.tab_ll.Y + 30))); //右

            bags.Add(new Line(new Point(Ball.tab_ur.X , Ball.tab_ll.Y), new Point(Ball.tab_ur.X - 30, Ball.tab_ll.Y))); //上
            bags.Add(new Line(new Point(Ball.tab_ll.X + 525, Ball.tab_ll.Y), new Point(Ball.tab_ll.X + 575, Ball.tab_ll.Y)));
            bags.Add(new Line(new Point(Ball.tab_ll.X + 25, Ball.tab_ll.Y), new Point(Ball.tab_ll.X - 5, Ball.tab_ll.Y))); 


            //Helper.SoundEffect("Hit04");


            Timer refresh_current_ball_timer = new Timer(); //新建一个Timer对象
            refresh_current_ball_timer.Interval = 1;//设定多少秒后行动，单位是毫秒
            refresh_current_ball_timer.Tick += new EventHandler(this.timer_handler);//到时所有执行的动作
            refresh_current_ball_timer.Start();//启动计时

            Ball.set_interval(0.01);


            /* 1. 进入游戏首先显示开始界面
             * 包括：背景图，开始游戏按钮，高分
             *
             * 2. 点击进入游戏关闭开始界面的东西，开始游戏
             *
             * 游戏中：
             * 1. 显示球台
             * 2. 显示多个球
             * 3. 刷新图片
             * */
            //place_line(3);
            place_triangle();
        }
        private void place_line(int number)
        {
            for (int i = 0; i < number - 1; i++)
                balls.Add(new Ball(300 + i * 60, 200 + i * 60, "Green", false));
            balls.Add(new Ball(300 + (number - 1) * 60, 200 + (number - 1) * 60, "Green", true));
        }
        private void place_triangle()
        {
            Point ll = Ball.tab_ll;
            Point ur = Ball.tab_ur;
            DP mid = new DP(ll.X + (ur.X - ll.X) * 0.5, ll.Y + (ur.Y - ll.Y) * 0.5);
            balls.Add(new Ball(ll.X + (ur.X - ll.X) * 0.2, ll.Y + (ur.Y - ll.Y) * 0.5, "Black", true));
            int diameter = Ball.get_diameter();
            for (int i = 1; i <= 4; i++)
                for (int j = 0; j < i; j++)
                {
                    string col;
                    if (i % 2 == 0)
                        col = "Red";
                    else
                        col = "Green";
                    balls.Add(new Ball(mid.X + i * (diameter + 35), mid.Y - i / 2 * (diameter + 35) + j * (diameter + 35), col, false));
                }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // 所有的渲染在这里进行
            Bitmap bufferBmp = new Bitmap(this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
            Graphics g = Graphics.FromImage(bufferBmp);

            if (GameFrame.page == "playing")
            {
                strength_bar.Visible = true;
                Font font = new Font("Arial", 20f, FontStyle.Bold);
                g.DrawString("目前共打了:" + total_pools.ToString() + "杆", font, new SolidBrush(Color.White), 50, 30);
                DrawTable(g);
                for (int i = 0; i < balls.Count(); i++)
                    balls[i].draw_ball(g);
                for (int i = 0; i < falling.Count(); i++)
                    falling[i].draw_ball(g);
                if(Ball.curr_hitting_ball != -1)
                    draw_trajectory(g);
                //if (Ball.curr_hitting_ball != -1)
                  //  draw_stick(Helper.to_point(GameFrame.get_ball_by_id(Ball.curr_hitting_ball).get_pos()), draw_direction, g);
            }
            else if (GameFrame.page == "initial")
            {
                // 1. 绘制背景
                Rectangle pending_wallpaper = new Rectangle((int)(0), (int)(0), 1600, 900);
                g.DrawImage(wallpaper, pending_wallpaper);

                int record = 0;
                if (File.Exists(".\\Records.txt") == false)
                {
                    StreamWriter sw = new StreamWriter(".\\Records.txt", true);
                    sw.WriteLine(65535);//要写入的数据
                    sw.Flush();
                    sw.Close();
                }
                FileStream fs = new FileStream(".\\Records.txt", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string str = sr.ReadLine();
                sr.Close();
                if (str.Length > 0)
                {
                    record = int.Parse(str);
                }
                //Console.WriteLine("read record" + record.ToString());
                if (record != 0 && record != 65535)
                {
                    Font font = new Font("Arial", 20f, FontStyle.Bold);
                    g.DrawString("当前最高记录：" + record.ToString() + "杆", font, new SolidBrush(Color.White), 1300, 750);
                }

                // draw_stick(Helper.to_point(new DP(400,400)) , 3.14, g);


                // TODO 转场淡入和淡出效果
            }
            else if (GameFrame.page == "win")
            {
                Rectangle win_back = new Rectangle((int)(0), (int)(0), this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
                g.DrawImage(win_background, win_back);

                Font font = new Font("Arial", 40f, FontStyle.Bold);
                if (new_record)
                    g.DrawString("只用了" + total_pools.ToString() + "杆\n   新纪录！", font, new SolidBrush(Color.White), 640, 460);
                else
                    g.DrawString("  共打了" + total_pools.ToString() + "杆\n没有创造新纪录", font, new SolidBrush(Color.White), 640, 460);

            }
            else if (GameFrame.page == "lose")
            {
                Rectangle lose_back = new Rectangle((int)(0), (int)(0), this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
                g.DrawImage(lose_background, lose_back);

            }

            e.Graphics.DrawImage(bufferBmp, 0, 0);
            g.Dispose();
            base.OnPaint(e);
        }

        // Events
        private void GameFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                for (int i = 0; i < balls.Count(); i++)
                    balls[i].stop();
        }

        private void GameFrame_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("左键按下事件" + e.X.ToString() + " " + e.Y.ToString());
            //Console.WriteLine(e.X.ToString() + " " + e.Y.ToString());
            if (GameFrame.page == "initial" && e.X > 630 && e.X < 960 && e.Y > 630 && e.Y < 700)
            {
                GameFrame.page = "playing";
                strength_bar.Visible = true;
            }

            if (GameFrame.page == "win" || GameFrame.page == "lose" && e.X > 580 && e.X < 1050 && e.Y > 700 && e.Y < 820)
            {
                total_pools = 0;
                GameFrame.page = "playing";
                while (balls.Count() != 0)
                    balls.Remove(balls[0]);
                place_triangle();
                strength_bar.Visible = true;
            }


            // 查找鼠标点在那个球里面
            Ball.curr_hitting_ball = -1;
            if (!balls_static()) // 如果当前局面有球在动，不允许击球
                return;
            for (int i = 0; i < balls.Count(); i++)
                if (balls[i].include_point(new Point(e.X, e.Y)))
                    Ball.curr_hitting_ball = balls[i].get_id();
            last_click_position = new Point(e.X, e.Y);
        }

        private void GameFrame_MouseUp(object sender, MouseEventArgs e)
        {
            // 根据当前在打的球，计算拖动距离和角度，给球速度和方向
            if (Ball.curr_hitting_ball == -1)
                return;
            if (GameFrame.get_ball_by_id(Ball.curr_hitting_ball).get_can_hit() == false)
                return;
            Helper.SoundEffect("Shot01");
            double dist = Helper.dist(last_click_position.X, last_click_position.Y, e.X, e.Y);
            double speed = dist * 6; // 这里设定速度和距离的关系
            //Console.WriteLine("当前角度" + direction.ToString());
            double direction = Helper.get_anti_direction(new DP(last_click_position), new DP(e.X, e.Y));

            get_ball_by_id(Ball.curr_hitting_ball).add_speed(speed, direction);

            total_pools++;
            Ball.curr_hitting_ball = -1;
            strength = 0;
        }
        private void GameFrame_MouseMove(object sender, MouseEventArgs e)
        {
            if (Ball.curr_hitting_ball == -1)
                return;
            if (GameFrame.get_ball_by_id(Ball.curr_hitting_ball).get_can_hit() == false)
                return;
            strength = Helper.dist(new DP(last_click_position), new DP(e.X, e.Y));
            if (strength > 600)
                strength = 600;
            strength = (int)(strength / 6);
            draw_direction = Helper.get_direction(new DP(last_click_position), new DP(e.X, e.Y));
            Ball curr_ball = GameFrame.get_ball_by_id(Ball.curr_hitting_ball);
            DP pos = curr_ball.get_pos();
            DP end = new DP(pos.X * 2 - e.X, pos.Y * 2 - e.Y);
            trajs = Helper.to_center_point(pos);
            traje = Helper.to_center_point(end);
            
            // Console.WriteLine(strength.ToString());
        }
        private void draw_trajectory(Graphics graphic)
        {
            Pen MyPen = new Pen(Color.White, 2f);
            MyPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            //使用DrawLine方法绘制直线
            graphic.DrawLine(MyPen, trajs, traje);

        }
        private void DrawTable(Graphics graphic)
        {
            // 1206 684
            Rectangle rectangle = new Rectangle(Ball.tab_ll.X - 55, Ball.tab_ll.Y - 55, 1206, 684);
            graphic.DrawImage(table, rectangle);
        }

        private void timer_handler(object sender, EventArgs e)
        {
            if (balls.Count() == 1)
                GameFrame.end_game("win");
            strength_bar.Value = (int)(strength);
            this.Invalidate();
        }
        public static Ball get_ball_by_id(int id)
        {
            for (int i = 0; i < balls.Count(); i++)
                if (balls[i].get_id() == id)
                    return balls[i];
            Console.WriteLine(id.ToString() + " [ERROR]没有发现对应的球");
            return balls[0];
        }
        private bool balls_static()
        {
            // 检查是否所有的球都静止
            for (int i = 0; i < balls.Count(); i++)
                if (balls[i].is_moving())
                    return false;
            return true;
        }
        private void draw_stick(Point pos, double direction,Graphics g) // 杆头的位置和拉动的方向
        {
            direction = direction / Math.PI * 180;
            Console.WriteLine("在" + pos.ToString() + "画杆,角度为" + direction.ToString() );
            Bitmap curr_stick = Helper.KiRotate(stick, (float)(direction), Color.Transparent);
            int posx = (int) (pos.X + 400 * Math.Cos(direction) );
            int posy = (int)(pos.Y - 400 * Math.Sin(direction));

            Rectangle stick_rec = new Rectangle(posx, posy, 400, 20);
            g.DrawImage(curr_stick, stick_rec);
        }


        public static void end_game(string reason)
        {
            //Console.WriteLine("游戏结束，" + reason);
            strength_bar.Visible = false;
            if (reason == "win")
            {
                Helper.SoundEffect("endwin");
                page = "win";
                if (!File.Exists(".\\Records.txt"))
                {
                    StreamWriter sw = new StreamWriter(".\\Records.txt", true);
                    sw.WriteLine(65534);//要写入的数据
                    sw.Flush();
                    sw.Close();
                }
                FileStream fs = new FileStream(".\\Records.txt", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string str = sr.ReadLine();
                sr.Close();
                int high_score = 65535;
                if (str.Length > 0)
                {
                    high_score = int.Parse(str);
                    if (total_pools < high_score)
                    {
                        new_record = true;
                        string[] lines = { total_pools.ToString() };
                        System.IO.File.WriteAllLines(".\\Records.txt", lines, Encoding.UTF8);
                    }

                    /*
                    StreamWriter sw = new StreamWriter(".\\Records.txt", true);
                    sw.WriteLine(high_score);//要写入的数据
                    sw.Flush();
                    sw.Close();
                    */
                }
            }
            else
            {
                Helper.SoundEffect("endfail");
                page = "lose";
            }
        }
    }

    public class Ball
    {
        public static int curr_hitting_ball = -1; // 根据鼠标按下标记当前在打哪个球
        static int count = 0; //记录到当前一共几个球了，根据这个给球的id，从0开始
        static double fade_speed = 400; // px/二次方s
        static double refresh_interval;
        static int diameter = 30;
        public static Point tab_ll = new Point(355, 155);
        public static Point tab_ur = new Point(1430, 700);
        double x, y;
        double speed, direction;
        string color;
        bool can_hit = false;
        bool falling = false;
        int id; // 每个球有唯一id
        Bitmap image;
        public Ball(Ball b)
        {
            DP center = b.get_pos();
            x = (int)(center.X);
            y = (int)(center.Y);
            direction = b.get_direction();
            speed = b.get_speed();
            can_hit = false;
            id = count++;
            color = b.get_color();

            if (color == "Black")
                image = Helper.LoadBmp("BlackBall");
            if (color == "White")
                image = Helper.LoadBmp("WhiteBall");
            if (color == "Green")
                image = Helper.LoadBmp("GreenBall");
            if (color == "Red")
                image = Helper.LoadBmp("RedBall");

            image.MakeTransparent(Color.Transparent);
        }
        public Ball(double xx, double yy, string col, bool hitter)
        {
            //Console.Write("Drawing" + col + "at" + xx.ToString() + " " + yy.ToString());
            can_hit = hitter;
            id = count++;
            x = xx;
            y = yy;
            color = col;
            if (color == "Black")
                image = Helper.LoadBmp("BlackBall");
            if (color == "White")
                image = Helper.LoadBmp("WhiteBall");
            if (color == "Green")
                image = Helper.LoadBmp("GreenBall");
            if (color == "Red")
                image = Helper.LoadBmp("RedBall");

            image.MakeTransparent(Color.Transparent);
        }
 
        public void draw_ball(Graphics graphic)
        {
            // 如果球有速度，在这里更新球的位置

            refresh_position();
            check_bump(); // 检测是否碰撞，并处理碰撞

            //Console.WriteLine(x.ToString() + " " + y.ToString());

            Rectangle rectangle = new Rectangle((int)(x), (int)(y), diameter, diameter);
            //Console.WriteLine(rectangle.ToString());
            graphic.DrawImage(image, rectangle);
            //graphic.DrawEllipse(new Pen(Color.Black, 3), rectangle.X, rectangle.Y - 1, rectangle.Width - 1, rectangle.Height);//圆形描边
        }
        private void refresh_position()
        {
            //Console.WriteLine(speed.ToString());
            double go_x = this.speed * refresh_interval * Math.Cos(this.direction);
            double go_y = this.speed * refresh_interval * Math.Sin(this.direction);
            x += go_x;
            y -= go_y;
            speed = speed - refresh_interval * fade_speed;
            if (speed < 50)
                speed = 0;

            if (falling)
                stop();
            
        }
        private bool check_bump()
        {
            // 检查台边和其他球的碰撞情况， 更新速度和角度
            // 1.检查和台边碰撞
            int bump_edge = check_bump_edge();
            if (bump_edge != -1)
            {
                Console.WriteLine("发生边缘碰撞，边为" + bump_edge.ToString());
                Helper.SoundEffect("Bank02"); // 1.1 处理和台边碰撞，播放声音，改变速度和方向
                process_bump_edge(bump_edge);
                int bump_bag_id = check_bump_bag(); // 1.2 如果和台边碰撞了继续检测和袋的碰撞
                if (bump_bag_id != -1)
                {
                    Helper.SoundEffect("fall");
                    if (can_hit)
                    {
                        GameFrame.end_game("black in bag");
                        return false;
                    }
                    process_bump_bag(bump_bag_id); //1.3 处理和袋的碰撞，删除球，添加入袋动画
                    return true;
                }
            }

            // 2. 检测和球的碰撞
            int bump_ball_id = check_bump_ball();

            if (bump_ball_id != -1) // 2.1 如果和球碰撞，改变两个球的速度和方向
            {
                Console.Write("球和球碰撞：" + id.ToString() + " " + bump_ball_id.ToString());
                if (speed > 500)
                {
                    Helper.SoundEffect("Hit05"); // TODO :不同的速度播放不同音量
                }
                else
                {
                    Helper.SoundEffect("Hit03");
                }
                Ball bumpted_ball = GameFrame.get_ball_by_id(bump_ball_id);
                process_bump_ball(bumpted_ball);
            }

            return false;
        }
 
        private void process_bump_ball(Ball bumpted_ball)
        {
            if (speed == 0)
                return;
            // 1. 将this的速度分解成x,y方向
            double this_speed_x = speed * Math.Cos(direction);
            double this_speed_y = speed * Math.Sin(direction);

            double bumpted_speed = bumpted_ball.get_speed();
            double bumpted_direction = bumpted_ball.get_direction();

            double that_speed_x = bumpted_speed * Math.Cos(direction);
            double that_speed_y = bumpted_speed * Math.Sin(direction);

            // 2. 计算圆心连线角度
            double link_direction = Helper.get_direction(new DP(x, y), bumpted_ball.get_pos());

            // 3. 连线角度的速度就是被撞的球要添加的速度，做出这个速度
            double this_link = this_speed_x * Math.Cos(link_direction) + this_speed_y * Math.Sin(link_direction);
            double that_link = that_speed_x * Math.Cos(link_direction) + that_speed_y * Math.Sin(link_direction);
            //double bumpt_speed = this_link + that_link;
            bumpted_ball.add_speed(0.8 * this_link, link_direction);
            bumpted_ball.add_speed(that_link, Helper.get_anti_direction(link_direction));

            this.add_speed(that_link * 0.8, link_direction);
            this.add_speed(this_link, Helper.get_anti_direction(link_direction));
            refresh_position();
            bumpted_ball.refresh_position();
            
        }
        private void process_bump_bag(int bag)
        {
            GameFrame.falling.Add(new FallingEffect(bag, color, 1000, get_pos() ));

            GameFrame.balls.Remove(GameFrame.get_ball_by_id(id));
            
            //todo 删掉自己这个球之后添加一个入袋动画
        }

        private void process_bump_edge(int edge)
        {
            speed = speed * 0.7;
            switch (edge)
            {
                case 1: //左
                    // TODO 在向左边缘，小角度，大速度碰撞有问题
                    if (direction < Math.PI)
                        direction = Math.PI - direction;
                    else
                        direction = Math.PI * 3 - direction;
                    break;
                case 2:
                    if (direction < Math.PI * 1.5)
                        direction = Math.PI * 2 - direction;
                    else
                        direction = Math.PI * 2 - direction;
                    break;
                case 3:
                    if (direction < Math.PI / 2)
                        direction = Math.PI - direction;
                    else
                        direction = Math.PI * 3 - direction;
                    break;
                case 4:
                    if (direction < Math.PI / 2)
                        direction = Math.PI * 2 - direction;
                    else
                        direction = Math.PI * 2 - direction;
                    break;
            }
        }

        private int check_bump_bag()
        {
            List<Line> bags = GameFrame.bags;
            for (int i = 0; i < bags.Count(); i++)
            {
                Line line = bags[i];
                Point a = line.a;
                Point b = line.b;
                int range_low, range_high;
                if (a.X == b.X)
                {
                    range_low = Math.Min(a.Y, b.Y);
                    range_high = Math.Max(a.Y, b.Y);
                    if (Math.Abs((int)(x) - a.X) <= 2 && (int)(y) >= range_low && (int)(y) <= range_high)
                        return i;
                }
                if (a.Y == b.Y)
                {
                    range_low = Math.Min(a.X, b.X);
                    range_high = Math.Max(a.X, b.X);
                    if (Math.Abs((int)(y) - a.Y) <= 2 && (int)(x) >= range_low && (int)(x) <= range_high)
                        return i;
                }
            }
            return -1;
        }
        private int check_bump_edge()
        {
            int intx = (int)(x);
            int inty = (int)(y);
            if (intx <= tab_ll.X) // 左
            {
                x = tab_ll.X + 1;
                return 1;
            }
            if (inty >= tab_ur.Y) // 下
            {
                y = tab_ur.Y - 1;
                return 2;
            }
            if (intx >= tab_ur.X) // 右
            {
                x = tab_ur.X - 1;
                return 3;
            }
            if (inty <= tab_ll.Y) // 上
            {
                y = tab_ll.Y + 1;
                return 4;
            }
            return -1;
        }
        private int check_bump_ball()
        {
            List<Ball> balls = GameFrame.balls;
            for (int i = 0; i < balls.Count(); i++)
            {
                if (balls[i].get_id() == id) // 不和自己碰
                    continue;
                if (dist(id, balls[i].get_id()) <= Ball.diameter)
                    return balls[i].get_id();
            }
            return -1;
        }
        public void add_speed(double add_speed, double add_direction)
        {
            //Console.WriteLine("------添加速度\n" + "添加一个速度，角度为：" + add_direction.ToString());
            double cur_speed_x = speed * Math.Cos(direction);
            double cur_speed_y = speed * Math.Sin(direction);

            double add_speed_x = add_speed * Math.Cos(add_direction);
            double add_speed_y = add_speed * Math.Sin(add_direction);
            //Console.WriteLine("添加速度分解，xy " + add_speed_x.ToString() + " " + add_speed_y.ToString());

            cur_speed_x += add_speed_x;
            cur_speed_y += add_speed_y;
            //Console.WriteLine("速度分别合成后,xy " + cur_speed_x.ToString() + " " + cur_speed_y.ToString());

            speed = Math.Sqrt(Math.Pow(cur_speed_x, 2) + Math.Pow(cur_speed_y, 2));
            direction = Math.Atan(cur_speed_y / cur_speed_x);
            //Console.WriteLine("atan角度:" + direction.ToString());
            if (cur_speed_x < 0 && cur_speed_y > 0)
                direction = Math.PI + direction;
            if (cur_speed_x < 0 && cur_speed_y < 0)
                direction = Math.PI + direction;

            //Console.WriteLine("合成速度方向:" + direction.ToString()+"\n-----------速度添加完了\n");

        }
        public static int get_diameter() { return diameter; }
        private void set_falling() { falling = true; }
        public bool get_can_hit() { return can_hit; }
        public string get_color() { return color; }
        public double get_speed() { return speed; }
        public double get_direction() { return direction; }
        public int get_id() { return id; }
        public void stop()
        {
            speed = 0;
            direction = 0;
        }


        public bool include_point(Point p)
        {
            if (Helper.dist(p.X, p.Y, x, y) <= diameter)
                return true;
            return false;
        }
        public static void set_interval(double itv)
        {
            refresh_interval = itv;
        }
        public bool is_moving()
        {
            if (speed > 0)
                return true;
            return false;
        }
        private double dist(int inda, int indb)
        {
            DP a = GameFrame.get_ball_by_id(inda).get_pos();
            DP b = GameFrame.get_ball_by_id(indb).get_pos();
            return Helper.dist(a.X, a.Y, b.X, b.Y);
        }
        public DP get_pos()
        {
            return new DP(x, y);
        }


    }

    public class DP
    {
        public double X, Y;
        public DP(double xx, double yy)
        {
            X = xx;
            Y = yy;
        }
        public DP(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
    }
    public class Line
    {
        public Point a;
        public Point b;
        public Line(Point aa, Point bb)
        {
            a = aa;
            b = bb;
        }
        public DP mid()
        {
            return new DP((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }
    }
    class Helper
    {
        public static Bitmap KiRotate(Bitmap bmp, float angle, Color bkColor)
        {
            int w = bmp.Width + 2;
            int h = bmp.Height + 2;
            // int w = (int)(bmp.Width * Math.Abs( Math.Cos(angle / 360 * Math.PI * 2)) );
            // int h = (int)(bmp.Height * Math.Abs( Math.Sin(angle / 360 * Math.PI * 2)) );
            Console.WriteLine(w.ToString() + h.ToString());
            PixelFormat pf;

            if (bkColor == Color.Transparent)
            {
                pf = PixelFormat.Format32bppArgb;
            }
            else
            {
                pf = bmp.PixelFormat;
            }

            Bitmap tmp = new Bitmap(w, h, pf);
            Graphics g = Graphics.FromImage(tmp);
            g.Clear(bkColor);
            g.DrawImageUnscaled(bmp, 1, 1);
            g.Dispose();

            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(0f, 0f, w, h));
            Matrix mtrx = new Matrix();
            mtrx.Rotate(angle);
            RectangleF rct = path.GetBounds(mtrx);

            Bitmap dst = new Bitmap((int)rct.Width, (int)rct.Height, pf);
            g = Graphics.FromImage(dst);
            g.Clear(bkColor);
            g.TranslateTransform(-rct.X, -rct.Y);
            g.RotateTransform(angle);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(tmp, 0, 0);
            g.Dispose();

            tmp.Dispose();

            return dst;
        }
        public static Point to_point(DP dp)
        {
            return new Point((int)(dp.X), (int)(dp.Y));
        }
        public static Point to_center_point(DP dp)
        {
            return new Point((int)(dp.X + Ball.get_diameter() / 2), (int)(dp.Y + Ball.get_diameter() / 2));
        }
        [DllImport("winmm")]
        public static extern bool PlaySound(string szSound, int hMod, int i);
        public static void SoundEffect(string waveName)
        {
            PlaySound(Application.StartupPath + "\\Sounds\\" + waveName + ".wav", 0, 1);
        }


        public static Bitmap LoadBmp(string bmpFileName)
        {
            Bitmap img = new Bitmap(Application.StartupPath + "\\Images\\" + bmpFileName + ".bmp");
            img.MakeTransparent(Color.Transparent);
            return img;
        }


        public static void background_music()
        {
            // todo 背景音乐放不出来
            SoundPlayer Player = new SoundPlayer();//首先NEW一个播放器
            Player.SoundLocation = ".\\Sounds\\Background.wav";
            Player.Load();
            Player.PlaySync();

        }


        public static double dist(double ax, double ay, double bx, double by)
        {
            double dist_x = ax - bx;
            double dist_y = ay - by;
            return Math.Sqrt(dist_x * dist_x + dist_y * dist_y);
        }
        public static double dist(DP a, DP b)
        {
            return dist(a.X, a.Y, b.X, b.Y);
        }

        public static double get_direction(DP s, DP e)
        {
            double dist = Helper.dist(s.X, s.Y, e.X, e.Y);
            double direction = Math.Asin((s.Y - e.Y) / dist);
            if (e.X < s.X)
                direction = Math.PI - direction;
            if (e.X > s.X && e.Y > s.Y)
                direction = Math.PI * 2 + direction;
            Console.WriteLine("击球，速度角度为:" + direction.ToString()); // 角度 0~2PI
            return direction;
        }
        public static double get_anti_direction(DP s, DP e)
        {
            double direction = get_direction(s, e);
            if (direction < Math.PI)
                return Math.PI + direction;
            return direction - Math.PI;
        }
        public static double get_anti_direction(double direction)
        {
            if (direction < Math.PI)
                return Math.PI + direction;
            return direction - Math.PI;
        }

    }
    public class FallingEffect
    {
        double x, y;
        int bag;
        string color;
        int time=0;
        int diameter = 32;
        Bitmap image;
        double direction = 0;
        double speed = 0;
        List<Point> bag_centers = new List<Point>();
        
        public FallingEffect(int b, string col, int t, DP curr_point)
        {
            Console.WriteLine("在" + b.ToString() + "创建新的入袋动画");
            bag_centers.Add(new Point(325, 130));
            bag_centers.Add(new Point(320, 740));
            bag_centers.Add(new Point(320, 740));
            bag_centers.Add(new Point(880, 740));
            bag_centers.Add(new Point(1470, 740));
            bag_centers.Add(new Point(1470, 740));
            bag_centers.Add(new Point(1460, 120));
            bag_centers.Add(new Point(1460, 120));
            bag_centers.Add(new Point(885, 120));
            bag_centers.Add(new Point(320, 130));

            direction=Helper.get_direction(curr_point, new DP(bag_centers[b]) );
            speed = 200;
            bag = b;
            color = col;
            time = t;
            if (color == "Black")
                image = Helper.LoadBmp("BlackBall");
            if (color == "White")
                image = Helper.LoadBmp("WhiteBall");
            if (color == "Green")
                image = Helper.LoadBmp("GreenBall");
            if (color == "Red")
                image = Helper.LoadBmp("RedBall");

            image.MakeTransparent(Color.Transparent);
            //Point place = bag_centers[bag];
            x = (int)(curr_point.X);
            y = (int)(curr_point.Y);

        }
        public void draw_ball(Graphics graphic)
        {
            refresh_position();
            int dia = (int) ( ( (speed / 200) * 0.3 + 0.7) * (double)(diameter) )  ;
            if (speed == 0)
                dia = 0;
            Rectangle rectangle = new Rectangle((int)(x), (int)(y), dia, dia);
            //Console.WriteLine("在" + rectangle.ToString() + "位置绘制掉落动画");
            graphic.DrawImage(image, rectangle);
        }
        private void refresh_position()
        {
            double go_x = this.speed * 0.05 * Math.Cos(this.direction);
            double go_y = this.speed * 0.05 * Math.Sin(this.direction);
            x += go_x;
            y -= go_y;
            speed = speed * 0.85;
            if (speed < 10)
                speed = 0;

        }
    }
}
