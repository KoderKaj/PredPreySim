using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RGB
{

    public partial class Form1 : Form
    {
        int maxX, maxY, size = 10, spriteMax = 1000;
        List<Food> foods = new List<Food> { };
        List<Food> fRemove = new List<Food> { };
        List<Food> fAdd = new List<Food> { };
        List<Hunter> hunters = new List<Hunter> { };
        List<Hunter> hAdd = new List<Hunter> { };
        List<Hunter> hRemove = new List<Hunter> { };
        public Form1()
        {
            InitializeComponent();
            maxX = this.ClientSize.Width;
            maxY = this.ClientSize.Height;
            foods.Add(new Food(10, 10));
            foods.Add(new Food(100, 100));
            foods.Add(new Food(130, 130));
            hunters.Add(new Hunter(200, 200));
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (Food food in foods)
            {
                g.FillEllipse(food.getCol(), new RectangleF(food.getX(), food.getY(), size, size)); ;
            }
            foreach (Hunter hunter in hunters)
            {
                g.FillEllipse(hunter.getCol(), new RectangleF(hunter.getX(), hunter.getY(), size, size));
            }
        }

        private void tick(object sender, EventArgs e)
        {
            maxX = this.ClientSize.Width;
            maxY = this.ClientSize.Height;
            foreach(Food food in foods)
            {
                fAdd.Add(food.move(foods, maxX, maxY));
            }
            if (fAdd.Count > 0 && foods.Count + hunters.Count < spriteMax)
            {
                foreach (Food food in fAdd)
                {
                    if (food != null) { foods.Add(food); }
                }
            }
            fAdd.Clear();
            foreach(Hunter hunter in hunters)
            {
                List<Sprite> sprites = hunter.move(foods, maxX, maxY);
                if (sprites.Count > 0)
                {
                    if (sprites.Count == 1)
                    {
                        hRemove.Add(hunter);
                    }
                    else
                    {
                        fRemove.Add((Food)sprites[1]);
                        if (sprites[0] != null)
                        {
                            hAdd.Add((Hunter)sprites[0]);
                        }
                    }
                }
            }
            if(fRemove.Count > 0)
            {
                foreach(Food food in fRemove)
                {
                    foods.Remove(food);
                }
                fRemove.Clear();
                if(hAdd.Count > 0 && foods.Count + hunters.Count < spriteMax)
                {
                    foreach(Hunter hunter in hAdd)
                    {
                        hunters.Add(hunter);
                    }
                    hAdd.Clear();
                }
            }
            else if(hRemove.Count > 0)
            {
                foreach(Hunter hunter in hRemove)
                {
                    hunters.Remove(hunter);
                }
                hRemove.Clear();
            }
            this.Invalidate();
        }
    }
    public class Sprite
    {
        public Sprite(){ }
        protected float velX, velY, x, y, speed = 15;
        protected int cooldown = 0;
        protected Brush colour;
        protected Food target;
        public Brush getCol()
        {return colour;}
        public void setXY(float setX, float setY)
        {
            x = setX;
            y = setY;
        }
        protected float[] targetFood(List<Food> foods)
        {
            target = null;
            float diffX = 0,
                diffY = 0,
                magnitude = 10,
                prevMag = 999,
                newDx, newDy;
            foreach (Food potential in foods)
            {
                if (potential != this)
                {
                    newDx = potential.x - x;
                    newDy = y - potential.y;
                    magnitude = (float)Math.Sqrt(newDx * newDx + newDy * newDy);
                    if (magnitude < prevMag)
                    {
                        prevMag = magnitude;
                        target = potential;
                        diffX = newDx;
                        diffY = newDy;
                    }
                }
            }
            diffX /= prevMag;
            diffY /= prevMag;
            return new float[] { magnitude, diffX, diffY };
        }
        protected void moveToFood(float[] dXdY, float maxX, float maxY)
        {
            x += dXdY[0] * speed;
            y -= dXdY[1] * speed;
            borderCheck(maxX, maxY);
        }
        protected void borderCheck(float maxX, float maxY)
        {
            float newX = float.NaN, newY = float.NaN;
            if(x < 0){newX = maxX - x;}
            else if(x > maxX){newX = x - maxX;}

            if(y < 0){newY = maxY - y;}
            else if(y > maxY){newY = y - maxY;}

            if ( !float.IsNaN(newX) ) { x = newX; }
            if ( !float.IsNaN(newY) ) { y = newY; }
        }
        public float getX()
        {return x;}
        public float getY()
        {return y;}
    }
    public class Food : Sprite
    {
        static Random rand = new Random();
        
        int maxDistance = 10;
        public Food(float newX, float newY) { setXY(newX, newY); colour = Brushes.Blue; cooldown = rand.Next(0,20);}
        private void randMove(float maxX, float maxY)
        {
            velX = rand.Next(-(int)(speed*1.1), (int)(speed*1.1));
            velY = (float)Math.Sqrt((speed*1.1) * (speed*1.1) - velX * velX);
            if (velX < -speed * 0.5 || velX > speed * 0.5)
            {
                velY *= -1;
            }
            x += velX;
            y += velY;
            borderCheck(maxX, maxY);
        }
        public Food move(List<Food> foods, float maxX, float maxY)
        {
            if (cooldown <= 0)
            {
                float[] magdXdY = targetFood(foods);
                if (magdXdY[0] <= maxDistance*0.5) { randMove(maxX, maxY); }
                else if (magdXdY[0] < maxDistance)
                {
                    cooldown = rand.Next(90,110);
                    return new Food(x + magdXdY[1] / 2, y + magdXdY[2] / 2);
                }
                else
                {
                    moveToFood(new float[2] { magdXdY[1], magdXdY[2] }, maxX, maxY);
                }
            }
            else
            {
                randMove(maxX, maxY);
                cooldown--;
            }
            return null;
        }
    }
    public class Hunter : Sprite
    {
        float eatDistance = 10, ttl = 200;
        public Hunter(float newX, float newY) { setXY(newX, newY); colour = Brushes.Red; cooldown = 40; speed = 25; }
        public List<Sprite> move(List<Food> foods, float maxX, float maxY)
        {
            List<Sprite> sprites = new List<Sprite> { };
            if (cooldown <= 0)
            {
                ttl--;
                if (ttl > 0)
                {
                    float[] magdXdY = targetFood(foods);
                    if (magdXdY[0] <= eatDistance && ttl > 50)
                    {
                        sprites.Add(new Hunter(x, y));
                        sprites.Add(target);
                        ttl = 200;
                        cooldown = 100;
                    }
                    else if (magdXdY[0] <= eatDistance)
                    {
                        sprites.Add(null);
                        sprites.Add(target);
                        ttl = 250;
                        cooldown = 50;
                    }
                    else
                    {
                        moveToFood(new float[2] { magdXdY[1], magdXdY[2] }, maxX, maxY);
                    }
                }
                else
                {
                    sprites.Add(this);
                }
            }
            else { cooldown--; }
            return sprites;
        }
    }
}
/*float diffX = (potential.x + potential.size / 2) - (x + size / 2);
float diffY = (y + size / 2) - (potential.y + potential.size / 2);
float magnitude = (float)Math.Sqrt(diffX * diffX + diffY * diffY);*/
