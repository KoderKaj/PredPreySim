using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RGB
{

    public partial class Form1 : Form
    {
        int maxX, maxY, size = 10, spriteMax = 1000;
        List<Food> foods = new List<Food> { };
        List<Food> fRemove = new List<Food> { };
        List<Food> fAdd = new List<Food> { };
        public Form1()
        {
            InitializeComponent();
            maxX = this.ClientSize.Width;
            maxY = this.ClientSize.Height;
            foods.Add(new Food(10, 10));
            foods.Add(new Food(50, 100));
            foods.Add(new Food(100, 100));
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (Food food in foods)
            {
                g.FillEllipse(food.getCol(), new RectangleF(food.getX(), food.getY(), size, size)); ;
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
            if (fAdd.Count > 0 && foods.Count < spriteMax)
            {
                foreach (Food food in fAdd)
                {
                    if (food != null) { foods.Add(food); }
                }
            }
            fAdd.Clear();
            this.Invalidate();
        }
    }
    public class Sprite
    {
        public Sprite(){ }
        protected float velX, velY, x, y, speed = 10;
        protected Brush colour;
        public Brush getCol()
        {return colour;}
        public void setXY(float setX, float setY)
        {
            x = setX;
            y = setY;
        }
        protected float[] targetFood(List<Food> foods)
        {
            Food target = null;
            float diffX = 0,
                diffY = 0,
                magnitude = 10,
                prevMag = 0,
                newDx, newDy;
            foreach (Food potential in foods)
            {
                newDx = potential.x - x;
                newDy = y - potential.y;
                magnitude = (float)Math.Sqrt(newDx * newDx + newDy * newDy);
                if (magnitude > prevMag)
                {
                    prevMag = magnitude;
                    target = potential;
                    diffX = newDx;
                    diffY = newDy;
                }
            }
            diffX /= magnitude;
            diffY /= magnitude;
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
        int maxDistance = 10, cooldown = 0;// rand.Next(0,20);
        public Food(float newX, float newY){setXY(newX,newY); colour = Brushes.Blue; }
        private void randMove(float maxX, float maxY)
        {
            velX = rand.Next(-(int)(speed*1.5), (int)(speed*1.5));
            velY = (float)Math.Sqrt((speed*1.5) * (speed*1.5) - velX * velX);
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
            if (cooldown == 0)
            {
                float[] magdXdY = targetFood(foods);
                if (magdXdY[0] <= maxDistance*0.2) { randMove(maxX, maxY); }
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
        public Hunter(float newX, float newY) { setXY(newX, newY); colour = Brushes.Red; }
        public Sprite[] move()
        {

            return null;
        }
    }
}
/*float diffX = (potential.x + potential.size / 2) - (x + size / 2);
float diffY = (y + size / 2) - (potential.y + potential.size / 2);
float magnitude = (float)Math.Sqrt(diffX * diffX + diffY * diffY);*/
