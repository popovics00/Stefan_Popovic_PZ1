using Stefan_Popovic_PZ1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Windows.Media.Animation;

namespace Stefan_Popovic_PZ1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            LoadFile(); //ucitavanje xml-a
            FindExtremes(); //pronalazenje ekstrema (x,y)
            InsertOnMap();  //popunjava se map[]

            InitializeComponent();

            funkcije.Add(ellipse);
            funkcije.Add(polygon);
            funkcije.Add(addText);

            DrawEntity();   //iscrtavanje na canvas i tooltip
            LineInitFunction();
            LineInitFunction2();

            FindLines();
            RoundTwo();

            Binding();
            DrawAllLines();
        }

        #region PRVI DIO

        #region promenljive
        public double noviX, noviY;
        SolidColorBrush colorStart = Brushes.Yellow;
        SolidColorBrush colorEnd = Brushes.Yellow;
        bool clicked = false;
        string helperA ="////";
        string helperB = "////";

        private List<SubstationEntity> substationEntities = new List<SubstationEntity>();
        private List<NodeEntity> nodeEntities = new List<NodeEntity>();
        private List<SwitchEntity> switchEntities = new List<SwitchEntity>();
        private List<LineEntity> lineEntities = new List<LineEntity>();
        private PowerEntity[,,] Map = new PowerEntity[300, 300, 300];
        private List<Ellipse> ellipses = new List<Ellipse>();

        private double MinX = double.MaxValue;
        private double MinY = double.MaxValue;

        private double MaxX = double.MinValue;
        private double MaxY = double.MinValue;

        private long tempId = 0;

        public MatrixElement[,] Lines = new MatrixElement[300, 300];
        private List<MatrixElement> TempList = new List<MatrixElement>();
        private List<MatrixElement> TempList1 = new List<MatrixElement>();
        public MatrixElement[,] Lines3 = new MatrixElement[300, 300];
        public MatrixElement[,] Lines2 = new MatrixElement[300, 300];
        public MatrixElement[,] Lines1 = new MatrixElement[300, 300];

        private Ellipse el1 = null;
        private Ellipse el2 = null;
        private Brush br1;
        private Brush br2;

        private bool check = true;
        public List<long> LineIdList = new List<long>();
        #endregion        

        public void LoadFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList xmlNodeList;

            xmlNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

            //ocitavam sve substation entitete i parsiram ih u listu objekata tog tipa
            foreach (XmlNode node in xmlNodeList)
            {
                SubstationEntity substationEntity = new SubstationEntity();
                substationEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                substationEntity.Name = node.SelectSingleNode("Name").InnerText;

                //komverzija utm to ToLatLon
                ToLatLon(double.Parse(node.SelectSingleNode("X").InnerText), double.Parse(node.SelectSingleNode("Y").InnerText), 34, out noviX, out noviY);

                substationEntity.X = noviX;
                substationEntity.Y = noviY;

                substationEntities.Add(substationEntity);
            }

            xmlNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");

            //ocitavamo i parsiramo nodove
            foreach (XmlNode node in xmlNodeList)
            {
                NodeEntity nodeEntity = new NodeEntity();
                nodeEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeEntity.Name = node.SelectSingleNode("Name").InnerText;

                ToLatLon(double.Parse(node.SelectSingleNode("X").InnerText), double.Parse(node.SelectSingleNode("Y").InnerText), 34, out noviX, out noviY);

                nodeEntity.X = noviX;
                nodeEntity.Y = noviY;

                nodeEntities.Add(nodeEntity);
            }

            xmlNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            //ocitavamo i parsiramo switch
            foreach (XmlNode node in xmlNodeList)
            {
                SwitchEntity switchEntity = new SwitchEntity();
                switchEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchEntity.Name = node.SelectSingleNode("Name").InnerText;
                switchEntity.Status = node.SelectSingleNode("Status").InnerText;

                ToLatLon(double.Parse(node.SelectSingleNode("X").InnerText), double.Parse(node.SelectSingleNode("Y").InnerText), 34, out noviX, out noviY);

                switchEntity.X = noviX;
                switchEntity.Y = noviY;

                switchEntities.Add(switchEntity);
            }

            xmlNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");

            //ocitavamo liniske entitete
            foreach (XmlNode node in xmlNodeList)
            {
                LineEntity lineEntity = new LineEntity();
                lineEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                lineEntity.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    lineEntity.IsUnderground = true;
                }
                else
                {
                    lineEntity.IsUnderground = false;
                }
                lineEntity.R = float.Parse(node.SelectSingleNode("R").InnerText);
                lineEntity.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                lineEntity.LineType = node.SelectSingleNode("LineType").InnerText;
                lineEntity.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                lineEntity.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                lineEntity.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                foreach (XmlNode pointNode in node.ChildNodes[9].ChildNodes) // 9 posto je Vertices 9. node u jednom line objektu
                {
                    Model.Point point = new Model.Point();

                    point.X = double.Parse(pointNode.SelectSingleNode("X").InnerText);
                    point.Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText);

                    ToLatLon(point.X, point.Y, 34, out noviX, out noviY);
                    point.X = noviX;
                    point.Y = noviY;
                    lineEntity.Vertices.Add(point);
                }

                lineEntities.Add(lineEntity);
            }
        }
        public void DrawEntity()
        {
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 300; j++)
                {
                    if (Map[i, j, 0] != null)
                    {

                        int index = 1;

                        while (Map[i, j, index] != null)
                        {
                            index++;

                        }

                        if (index == 1)
                        {
                            Ellipse ellipse = new Ellipse();
                            ellipse.Width = 6;
                            ellipse.Height = 6;

                            string toolTip = "";

                            if (Map[i, j, 0].GetType().ToString() == "Stefan_Popovic_PZ1.Model.SubstationEntity")
                            {
                                toolTip += "SubstationEntity\n" + "Name: " + Map[i, j, 0].Name + "\nID: " + Map[i, j, 0].Id + "\n";
                                ellipse.Fill = Brushes.Red;
                            }
                            else if (Map[i, j, 0].GetType().ToString() == "Stefan_Popovic_PZ1.Model.NodeEntity")
                            {
                                toolTip += "NodeEntity\n" + "Name: " + Map[i, j, 0].Name + "\nID: " + Map[i, j, 0].Id + "\n";
                                ellipse.Fill = Brushes.Green;
                            }
                            else if (Map[i, j, 0].GetType().ToString() == "Stefan_Popovic_PZ1.Model.SwitchEntity")
                            {
                                toolTip += "SwitchEntity\n" + "Name: " + Map[i, j, 0].Name + "\nID: " + Map[i, j, 0].Id + "\n";
                                ellipse.Fill = Brushes.Black;
                            }

                            ellipse.ToolTip = toolTip;
                            ellipse.Stroke = Brushes.Black;
                            ellipse.StrokeThickness = 1;

                            Canvas.SetBottom(ellipse, i * 6);
                            Canvas.SetLeft(ellipse, j * 6);

                            ellipses.Add(ellipse);
                            Canvass.Children.Add(ellipse);
                        }
                        else
                        {
                            Ellipse ellipse1 = new Ellipse();
                            ellipse1.Width = 6;
                            ellipse1.Height = 6;
                            ellipse1.Fill = Brushes.DarkGray;

                            string toolTip1 = "";

                            for (int k = 0; k < index; k++)
                            {
                                toolTip1 += "Item" + (k + 1) + "\n";

                                if (Map[i, j, 0].GetType().ToString() == "Stefan_Popovic_PZ1.Model.SubstationEntity")
                                {
                                    toolTip1 += "SubstationEntity\n" + "Name: " + Map[i, j, 0].Name + "\nID: " + Map[i, j, 0].Id + "\n";
                                    ellipse1.Fill = Brushes.Red;
                                }
                                else if (Map[i, j, 0].GetType().ToString() == "Stefan_Popovic_PZ1.Model.NodeEntity")
                                {
                                    toolTip1 += "NodeEntity\n" + "Name: " + Map[i, j, 0].Name + "\nID: " + Map[i, j, 0].Id + "\n";
                                    ellipse1.Fill = Brushes.Green;
                                }
                                else if (Map[i, j, 0].GetType().ToString() == "Stefan_Popovic_PZ1.Model.SwitchEntity")
                                {
                                    toolTip1 += "SwitchEntity\n" + "Name: " + Map[i, j, 0].Name + "\nID: " + Map[i, j, 0].Id + "\n";
                                    ellipse1.Fill = Brushes.Black;
                                }
                            }
                            ellipse1.ToolTip = toolTip1;
                            ellipse1.Stroke = Brushes.Black;
                            ellipse1.StrokeThickness = 1;
                            //ellipse1.Name = "ee";

                            Canvas.SetBottom(ellipse1, i * 6);
                            Canvas.SetLeft(ellipse1, j * 6);

                            ellipses.Add(ellipse1);
                            Canvass.Children.Add(ellipse1);
                        }
                    }
                }
            }
        }
        public void InsertOnMap()
        {
            foreach (SubstationEntity subEnt in substationEntities)
            {
                double positionOfX = (subEnt.X - MinX) / ((MaxX - MinX) / 299);
                double positionOfY = (subEnt.Y - MinY) / ((MaxY - MinY) / 299);

                for (int i = 0; i < 300; i++)
                {
                    if (Map[(int)positionOfX, (int)positionOfY, i] == null)
                    {
                        Map[(int)positionOfX, (int)positionOfY, i] = subEnt;
                        //System.Diagnostics.Debug.WriteLine($"{(int)positionOfX} {(int)positionOfY}");
                        break;
                    }

                }
            }


            foreach (NodeEntity nodEnt in nodeEntities)
            {
                double positionOfX = (nodEnt.X - MinX) / ((MaxX - MinX) / 299);
                double positionOfY = (nodEnt.Y - MinY) / ((MaxY - MinY) / 299);

                for (int i = 0; i < 300; i++)
                {
                    if (Map[(int)positionOfX, (int)positionOfY, i] == null)
                    {
                        Map[(int)positionOfX, (int)positionOfY, i] = nodEnt;
                        break;
                    }
                }
            }

            foreach (SwitchEntity swEnt in switchEntities)
            {
                double positionOfX = (swEnt.X - MinX) / ((MaxX - MinX) / 299);
                double positionOfY = (swEnt.Y - MinY) / ((MaxY - MinY) / 299);

                for (int i = 0; i < 300; i++)
                {
                    if (Map[(int)positionOfX, (int)positionOfY, i] == null)
                    {
                        Map[(int)positionOfX, (int)positionOfY, i] = swEnt;
                        //System.Diagnostics.Debug.WriteLine($"{(int)positionOfX} {(int)positionOfY}");
                        break;
                    }
                }


            }
        }
        public void FindExtremes()
        {

            foreach (SubstationEntity subEnt in substationEntities)
            {
                if (subEnt.X > MaxX)
                    MaxX = subEnt.X;
                else if (subEnt.X < MinX)
                    MinX = subEnt.X;

                if (subEnt.Y > MaxY)
                    MaxY = subEnt.Y;
                else if (subEnt.Y < MinY)
                    MinY = subEnt.Y;
            }


            foreach (NodeEntity nodeEnt in nodeEntities)
            {
                if (nodeEnt.X > MaxX)
                    MaxX = nodeEnt.X;
                else if (nodeEnt.X < MinX)
                    MinX = nodeEnt.X;

                if (nodeEnt.Y > MaxY)
                    MaxY = nodeEnt.Y;
                else if (nodeEnt.Y < MinY)
                    MinY = nodeEnt.Y;
            }

            foreach (SwitchEntity SwEnt in switchEntities)
            {
                if (SwEnt.X > MaxX)
                    MaxX = SwEnt.X;
                else if (SwEnt.X < MinX)
                    MinX = SwEnt.X;

                if (SwEnt.Y > MaxY)
                    MaxY = SwEnt.Y;
                else if (SwEnt.Y < MinY)
                    MinY = SwEnt.Y;
            }
        }
        public void LineInitFunction()
        {
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 300; j++)
                {
                    Lines1[i, j] = new MatrixElement()
                    {
                        Id = new Tuple<int, int>(i, j)
                        
                    };
                    //System.Diagnostics.Debug.WriteLine($"{Lines1[i, j]}");
                    Lines2[i, j] = new MatrixElement()
                    {
                        Id = new Tuple<int, int>(i, j)
                    };

                    Lines3[i, j] = new MatrixElement()
                    {
                        Id = new Tuple<int, int>(i, j)
                    };


                    if (Map[i, j, 0] != null)
                    {

                        Lines1[i, j].Type = FieldType.Obstacle;
                        Lines2[i, j].Type = FieldType.Obstacle;
                        Lines3[i, j].Type = FieldType.Obstacle;
                    }
                    else
                    {

                        Lines1[i, j].Type = FieldType.Free;
                        Lines2[i, j].Type = FieldType.Free;
                        Lines3[i, j].Type = FieldType.Free;
                    }
                }
            }
        }
        public void LineInitFunction2()
        {
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 300; j++)
                {
                    if (check == true)
                    {
                        Lines[i, j] = Lines1[i, j];
                        if (Lines[i, j].Type == FieldType.Visited)
                            Lines[i, j].Type = FieldType.Free;
                    }
                    else
                    {
                        Lines[i, j] = Lines2[i, j];
                       // System.Diagnostics.Debug.WriteLine($"{Lines[i, j]}");
                        if (Lines[i, j].Type == FieldType.Visited)
                            Lines[i, j].Type = FieldType.Free;
                    }
                }
            }
        }
        public void FindLines()
        {
            foreach (LineEntity lineEnt in lineEntities)
            {
                long firstEnd = lineEnt.FirstEnd;
                long secondEnd = lineEnt.SecondEnd;

                PowerEntity peFirst = null;
                PowerEntity peSecond = null;

                FindPoints(firstEnd, secondEnd, out peFirst, out peSecond);

                if (peFirst != null && peSecond != null)
                {
                    tempId = lineEnt.Id;

                    double positionOfX1 = (peFirst.X - MinX) / ((MaxX - MinX) / 299);
                    double positionOfY1 = (peFirst.Y - MinY) / ((MaxY - MinY) / 299);

                    double positionOfX2 = (peSecond.X - MinX) / ((MaxX - MinX) / 299);
                    double positionOfY2 = (peSecond.Y - MinY) / ((MaxY - MinY) / 299);

                    if ((int)positionOfX1 == (int)positionOfX2 && (int)positionOfY1 == (int)positionOfY2)
                    {
                        continue;                        
                    }
                    else
                    {
                        ChoseTheShortestPath((int)positionOfX1, (int)positionOfY1, (int)positionOfX2, (int)positionOfY2);
                        
                    }
                }
            }
        }
        public void FindPoints(long firstEnd, long secondEnd, out PowerEntity peFirst, out PowerEntity peSecond)
        {
            peFirst = null;
            peSecond = null;

            foreach (SubstationEntity subEnt in substationEntities)
            {

                if (peFirst == null)
                {

                    if (subEnt.Id == firstEnd)
                    {
                        peFirst = subEnt;
                    }
                }
                if (peSecond == null)
                {
                    if (subEnt.Id == secondEnd)
                        peSecond = subEnt;

                }
            }

            foreach (NodeEntity nodeEnt in nodeEntities)
            {
                if (peFirst == null)
                {
                    if (nodeEnt.Id == firstEnd)
                        peFirst = nodeEnt;

                }
                if (peSecond == null)
                {
                    if (nodeEnt.Id == secondEnd)
                        peSecond = nodeEnt;

                }
            }

            foreach (SwitchEntity swEnt in switchEntities)
            {
                if (peFirst == null)
                {
                    if (swEnt.Id == firstEnd)
                        peFirst = swEnt;

                }
                if (peSecond == null)
                {
                    if (swEnt.Id == secondEnd)
                        peSecond = swEnt;

                }
            }
        }
        public void ChoseTheShortestPath(int StartPosX, int StartPosY, int EndPosX, int EndPosY)
        {
            Lines[StartPosX, StartPosY].Step = 0;
            Lines[StartPosX, StartPosY].Type = FieldType.Visited;
            Lines[EndPosX, EndPosY].Type = FieldType.Free;

            Tuple<int, int> start = new Tuple<int, int>(StartPosX, StartPosY);

            Tuple<int, int> end = Lines[EndPosX, EndPosY].Id;
            bool found = false;

            TempList.Add(Lines[StartPosX, StartPosY]);
            while (TempList.Count != 0)
            {
                foreach (MatrixElement matEl in TempList)
                {
                    if (matEl.Id == end)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    foreach (MatrixElement meEl in TempList)
                    {
                        CheckAround(meEl);
                    }

                    TempList = TempList1;
                    TempList1 = new List<MatrixElement>();
                }
                else
                    break;
            }
            if (found == true)
            {
                int m = end.Item1;
                int n = end.Item2;
                int m2 = 0;
                int n2 = 0;

                while (m != start.Item1 || n != start.Item2)
                {
                    if (check == true)
                    {
                        int X = Lines[m, n].PreviousStep.Item1;
                        int Y = Lines[m, n].PreviousStep.Item2;
                        Lines1[X, Y].Type = FieldType.Line;
                        Lines1[X, Y].Direction = Lines[X, Y].Direction;
                        Lines1[X, Y].PreviousStep = Lines[X, Y].PreviousStep;
                        Lines1[X, Y].listId.Add(tempId);
                        SetpositionLook(Lines1[m, n]);
                    }
                    else
                    {
                        int X = Lines[m, n].PreviousStep.Item1;
                        int Y = Lines[m, n].PreviousStep.Item2;
                        Lines2[X, Y].Type = FieldType.Line;
                        Lines2[X, Y].Direction = Lines[X, Y].Direction;
                        Lines2[X, Y].PreviousStep = Lines[X, Y].PreviousStep;
                        Lines2[X, Y].listId.Add(tempId);
                        SetpositionLook(Lines2[m, n]);
                    }
                    m2 = m;
                    n2 = n;
                    m = Lines[m2, n2].PreviousStep.Item1;
                    n = Lines[m2, n2].PreviousStep.Item2;
                }
            }
            else
            {
                if (check)
                {
                    LineIdList.Add(tempId);
                }
            }
            LineInitFunction2();
            if (check)
            {
                Lines1[end.Item1, end.Item2].Type = FieldType.Obstacle;
                Lines1[start.Item1, start.Item2].Type = FieldType.Obstacle;
            }
            else
            {
                Lines2[end.Item1, end.Item2].Type = FieldType.Obstacle;
                Lines2[start.Item1, start.Item2].Type = FieldType.Obstacle;
            }

            TempList = new List<MatrixElement>();
            TempList1 = new List<MatrixElement>();

            //From UTM to Latitude and longitude in decimal         
        }
        public void SetpositionLook(MatrixElement el)
        {
            if (check == true)
            {
                if (el.Direction == Previous.Left)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Left)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.horizontally;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.horizontally;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Up)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.Lposition;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.Lposition;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Down)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.upsidedownL;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.upsidedownL;
                    }
                }
                else if (el.Direction == Previous.Right)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Right)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.horizontally;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.horizontally;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Up)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.othersideL;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.othersideL;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Down)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.totalRotL;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.totalRotL;
                    }
                }
                else if (el.Direction == Previous.Up)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Up)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.straight;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.straight;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Right)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.upsidedownL;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.upsidedownL;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Left)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.totalRotL;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.totalRotL;
                    }

                }
                else if (el.Direction == Previous.Down)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Down)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.straight;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.straight;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Right)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.Lposition;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.Lposition;
                    }
                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Left)
                    {
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.othersideL;
                        Lines1[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint1 = positionLook.othersideL;
                    }
                }
            }
            else
            {
                if (el.Direction == Previous.Left)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Left)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.horizontally;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Down)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.upsidedownL;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Up)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.Lposition;
                }

                else if (el.Direction == Previous.Right)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Right)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.horizontally;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Up)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.othersideL;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Down)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.totalRotL;
                }

                else if (el.Direction == Previous.Up)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Up)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.straight;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Right)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.upsidedownL;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Left)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.totalRotL;
                }
                else if (el.Direction == Previous.Down)
                {
                    if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Down)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.straight;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Right)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.Lposition;

                    else if (Lines[el.PreviousStep.Item1, el.PreviousStep.Item2].Direction == Previous.Left)
                        Lines2[el.PreviousStep.Item1, el.PreviousStep.Item2].Paint = positionLook.othersideL;

                }

            }

        }
        public void CheckAround(MatrixElement CurrentEl)
        {
            if (CurrentEl.Id.Item1 + 1 < 300)
            {

                if (Lines[CurrentEl.Id.Item1 + 1, CurrentEl.Id.Item2].Type == FieldType.Free)
                {
                    Lines[CurrentEl.Id.Item1 + 1, CurrentEl.Id.Item2].Type = FieldType.Visited;
                    Lines[CurrentEl.Id.Item1 + 1, CurrentEl.Id.Item2].Step = CurrentEl.Step + 1;
                    Lines[CurrentEl.Id.Item1 + 1, CurrentEl.Id.Item2].PreviousStep = CurrentEl.Id;
                    Lines[CurrentEl.Id.Item1 + 1, CurrentEl.Id.Item2].Direction = Previous.Down;
                    TempList1.Add(Lines[CurrentEl.Id.Item1 + 1, CurrentEl.Id.Item2]);

                }
            }
            if (CurrentEl.Id.Item1 - 1 >= 0)
            {
                if (Lines[CurrentEl.Id.Item1 - 1, CurrentEl.Id.Item2].Type == FieldType.Free)
                {
                    Lines[CurrentEl.Id.Item1 - 1, CurrentEl.Id.Item2].Type = FieldType.Visited;
                    Lines[CurrentEl.Id.Item1 - 1, CurrentEl.Id.Item2].Step = CurrentEl.Step + 1;
                    Lines[CurrentEl.Id.Item1 - 1, CurrentEl.Id.Item2].PreviousStep = CurrentEl.Id;
                    Lines[CurrentEl.Id.Item1 - 1, CurrentEl.Id.Item2].Direction = Previous.Up;
                    TempList1.Add(Lines[CurrentEl.Id.Item1 - 1, CurrentEl.Id.Item2]);
                }
            }
            if (CurrentEl.Id.Item2 + 1 < 300)
            {
                if (Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 + 1].Type == FieldType.Free)
                {

                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 + 1].Type = FieldType.Visited;
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 + 1].Step = CurrentEl.Step + 1;
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 + 1].PreviousStep = CurrentEl.Id;
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 + 1].Direction = Previous.Left;
                    TempList1.Add(Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 + 1]);
                }
            }


            if (CurrentEl.Id.Item2 - 1 >= 0)
            {
                if (Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 - 1].Type == FieldType.Free)
                {
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 - 1].Type = FieldType.Visited;
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 - 1].Step = CurrentEl.Step + 1;
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 - 1].PreviousStep = CurrentEl.Id;
                    Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 - 1].Direction = Previous.Right;
                    TempList1.Add(Lines[CurrentEl.Id.Item1, CurrentEl.Id.Item2 - 1]);
                }
            }
        }
        public void RoundTwo()
        {

            foreach (LineEntity lineEnt in lineEntities)
            {
                
                if (LineIdList.Contains(lineEnt.Id))
                {
                    check = false;

                    long firstEnd = lineEnt.FirstEnd;
                    long secondEnd = lineEnt.SecondEnd;

                    PowerEntity peFirst = null;
                    PowerEntity peSecond = null;

                    FindPoints(firstEnd, secondEnd, out peFirst, out peSecond);
                    LineInitFunction2();

                    if (peFirst != null && peSecond != null)
                    {
                        tempId = lineEnt.Id;
                        double positionOfX = (peFirst.X - MinX) / ((MaxX - MinX) / 299);
                        double positionOfY = (peFirst.Y - MinY) / ((MaxY - MinY) / 299);

                        double positionOfX2 = (peSecond.X - MinX) / ((MaxX - MinX) / 299);
                        double positionOfY2 = (peSecond.Y - MinY) / ((MaxY - MinY) / 299);

                        if ((int)positionOfX == (int)positionOfX2 && (int)positionOfY == (int)positionOfY2)
                        {
                            continue;
                        }
                        else
                        {
                            ChoseTheShortestPath((int)positionOfX, (int)positionOfY, (int)positionOfX2, (int)positionOfY2);
                        }
                    }
                }
            }
            LineIdList = new List<long>();
        }
        public void Binding()
        {
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 300; j++)
                {

                    if ((Lines1[i, j].Type == FieldType.Free || Lines1[i, j].Type == FieldType.Visited) && Lines2[i, j].Type == FieldType.Line)
                    {
                        positionLook temp = Lines2[i, j].Paint;
                        Lines3[i, j] = Lines2[i, j];
                        Lines3[i, j].Paint = temp; 
                    }
                    else if ((Lines2[i, j].Type == FieldType.Free || Lines2[i, j].Type == FieldType.Visited) && Lines1[i, j].Type == FieldType.Line)
                    { 
                        positionLook temp = Lines1[i, j].Paint;
                        Lines3[i, j] = Lines1[i, j];
                        Lines3[i, j].Paint = temp;
                    }
                    else if (Lines1[i, j].Type == FieldType.Line && Lines2[i, j].Type == FieldType.Line)
                    {
                        if (Lines1[i, j].Paint == Lines2[i, j].Paint)
                        {
                            Lines3[i, j] = Lines1[i, j];
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }
                        else if ((Lines1[i, j].Paint == positionLook.Lposition && Lines2[i, j].Paint == positionLook.othersideL) || (Lines2[i, j].Paint == positionLook.Lposition && Lines1[i, j].Paint == positionLook.othersideL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.upsidedownT;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }
                        else if ((Lines1[i, j].Paint == positionLook.Lposition && Lines2[i, j].Paint == positionLook.horizontally) || (Lines2[i, j].Paint == positionLook.Lposition && Lines1[i, j].Paint == positionLook.horizontally))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.upsidedownT;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.horizontally && Lines2[i, j].Paint == positionLook.othersideL) || (Lines2[i, j].Paint == positionLook.horizontally && Lines1[i, j].Paint == positionLook.othersideL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.upsidedownT;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }
                        else if ((Lines1[i, j].Paint == positionLook.horizontally && Lines2[i, j].Paint == positionLook.straight) || (Lines2[i, j].Paint == positionLook.horizontally && Lines1[i, j].Paint == positionLook.straight))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.plus;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.Lposition && Lines2[i, j].Paint == positionLook.totalRotL) || (Lines2[i, j].Paint == positionLook.Lposition && Lines1[i, j].Paint == positionLook.totalRotL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.plus;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }
                        else if ((Lines1[i, j].Paint == positionLook.upsidedownL && Lines2[i, j].Paint == positionLook.othersideL) || (Lines2[i, j].Paint == positionLook.upsidedownL && Lines1[i, j].Paint == positionLook.othersideL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.plus;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }
                        else if ((Lines1[i, j].Paint == positionLook.upsidedownL && Lines2[i, j].Paint == positionLook.totalRotL) || (Lines2[i, j].Paint == positionLook.upsidedownL && Lines1[i, j].Paint == positionLook.totalRotL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.Tposition;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.upsidedownL && Lines2[i, j].Paint == positionLook.horizontally) || (Lines2[i, j].Paint == positionLook.upsidedownL && Lines1[i, j].Paint == positionLook.horizontally))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.Tposition;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.horizontally && Lines2[i, j].Paint == positionLook.totalRotL) || (Lines2[i, j].Paint == positionLook.horizontally && Lines1[i, j].Paint == positionLook.totalRotL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.Tposition;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.upsidedownL && Lines2[i, j].Paint == positionLook.Lposition) || (Lines2[i, j].Paint == positionLook.upsidedownL && Lines1[i, j].Paint == positionLook.Lposition))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.notclockwise90T;

                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.upsidedownL && Lines2[i, j].Paint == positionLook.straight) || (Lines2[i, j].Paint == positionLook.upsidedownL && Lines1[i, j].Paint == positionLook.straight))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.notclockwise90T;

                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.straight && Lines2[i, j].Paint == positionLook.Lposition) || (Lines2[i, j].Paint == positionLook.straight && Lines1[i, j].Paint == positionLook.Lposition))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.notclockwise90T;

                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.othersideL && Lines2[i, j].Paint == positionLook.totalRotL) || (Lines2[i, j].Paint == positionLook.othersideL && Lines1[i, j].Paint == positionLook.totalRotL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.clockwise90T;

                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.othersideL && Lines2[i, j].Paint == positionLook.straight) || (Lines2[i, j].Paint == positionLook.othersideL && Lines1[i, j].Paint == positionLook.straight))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.clockwise90T;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                        else if ((Lines1[i, j].Paint == positionLook.straight && Lines2[i, j].Paint == positionLook.totalRotL) || (Lines2[i, j].Paint == positionLook.straight && Lines1[i, j].Paint == positionLook.totalRotL))
                        {
                            Lines3[i, j] = Lines1[i, j];
                            Lines3[i, j].Paint = positionLook.clockwise90T;
                            foreach (var item in Lines2[i, j].listId)
                            {
                                Lines3[i, j].listId.Add(item);
                            }
                        }

                    }
                }
            }
        }
        public void DrawAllLines()
        {
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 300; j++)
                {
                    if (Lines3[i, j].Type == FieldType.Line)
                    {

                        Line line1 = new Line();
                        line1.StrokeThickness = 0.4;
                        line1.Stroke = Brushes.Black;
                        line1.X1 = j * 6 + 3;
                        line1.X2 = j * 6 + 3;
                        line1.Y1 = (299 - i) * 6 - 0.1;
                        line1.Y2 = (299 - i) * 6 + 3.1;
                        

                        Line line2 = new Line();
                        line2.StrokeThickness = 0.4;
                        line2.Stroke = Brushes.Black;
                        line2.X1 = j * 6 + 3;
                        line2.X2 = j * 6 + 3;
                        line2.Y1 = (299 - i) * 6 + 3.1;
                        line2.Y2 = (299 - i) * 6 + 6;

                        Line line3 = new Line();
                        line3.StrokeThickness = 0.4;
                        line3.Stroke = Brushes.Black;
                        line3.X1 = j * 6;
                        line3.X2 = j * 6 + 3.1;
                        line3.Y1 = (299 - i) * 6 + 3;
                        line3.Y2 = (299 - i) * 6 + 3;

                        Line line4 = new Line();
                        line4.StrokeThickness = 0.4;
                        line4.Stroke = Brushes.Black;
                        line4.X1 = j * 6 + 3;
                        line4.X2 = j * 6 + 6;
                        line4.Y1 = (299 - i) * 6 + 3;
                        line4.Y2 = (299 - i) * 6 + 3;

                        List<Line> List1 = new List<Line>();
                        List1.Add(line1);
                        List1.Add(line2);
                        List1.Add(line3);
                        List1.Add(line4);

                        Ellipse el1 = new Ellipse();
                        el1.Width = 3;
                        el1.Height = 3;
                        el1.Fill = Brushes.DarkOrange;

                        el1.ToolTip = "";

                        foreach (var item in Lines3[i, j].listId)
                        {
                            el1.ToolTip += item.ToString() + " \n";

                        }

                        el1.StrokeThickness = 0.8;
                        el1.Stroke = Brushes.Black;
                        Canvas.SetBottom(el1, i * 6 + 1);
                        Canvas.SetLeft(el1, j * 6 + 1);

                        if (Lines3[i, j].Paint == positionLook.horizontally)
                        {
                            string toolTip = "";
                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                toolTip += "Id: " + item1.ToString() + "\n";
                                toolTip += "Name: " + AddNameLines(item1) + "\n";
                                toolTip += "Start: " + addStartOfLine(item1) + "\n";
                                toolTip += "End: " + addEndOfLine(item1) + "\n";

                            }

                            line3.ToolTip = toolTip;
                            line4.ToolTip = toolTip;
                            line3.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line4.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line3.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            line4.MouseLeftButtonDown += Linija_MouseRightButtonDown;


                            ContextMenu menu = new ContextMenu();

                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                MenuItem m1 = new MenuItem();

                                m1.Header = item1;
                                m1.Click += Find;
                                menu.Items.Add(m1);
                            }

                            line3.ContextMenu = menu;
                            line4.ContextMenu = menu;
                            Canvass.Children.Add(line3);
                            Canvass.Children.Add(line4);
                        }
                        else if (Lines3[i, j].Paint == positionLook.straight)
                        {
                            string toolTip = "";
                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                toolTip += "Id: " + item1.ToString() + "\n";
                                toolTip += "Name: " + AddNameLines(item1) + "\n";
                                toolTip += "Start: " + addStartOfLine(item1) + "\n";
                                toolTip += "End: " + addEndOfLine(item1) + "\n";
                            }
                            line1.ToolTip = toolTip;
                            line2.ToolTip = toolTip;
                            line1.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line2.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line1.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            line2.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            ContextMenu menu = new ContextMenu();

                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                MenuItem m1 = new MenuItem();
                                m1.Header = item1;
                                m1.Click += Find;
                                menu.Items.Add(m1);
                            }
                            line1.ContextMenu = menu;
                            line2.ContextMenu = menu;
                            Canvass.Children.Add(line1);
                            Canvass.Children.Add(line2);
                        }
                        else if (Lines3[i, j].Paint == positionLook.Lposition)
                        {
                            string toolTip = "";
                            foreach (var item in Lines3[i, j].listId)
                            {
                                toolTip += "Id: " + item.ToString() + "\n";
                                toolTip += "Name: " + AddNameLines(item) + "\n";
                                toolTip += "Start: " + addStartOfLine(item) + "\n";
                                toolTip += "End: " + addEndOfLine(item) + "\n";
                            }

                            line1.ToolTip = toolTip;
                            line4.ToolTip = toolTip;
                            line1.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line4.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line1.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            line4.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            ContextMenu menu = new ContextMenu();

                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                MenuItem m1 = new MenuItem();
                                m1.Header = item1;
                                m1.Click += Find;
                                menu.Items.Add(m1);
                            }

                            line1.ContextMenu = menu;
                            line4.ContextMenu = menu;
                            Canvass.Children.Add(line1);
                            Canvass.Children.Add(line4);
                        }
                        else if (Lines3[i, j].Paint == positionLook.othersideL)
                        {
                            string toolTip = "";

                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                toolTip += "Id: " + item1.ToString() + "\n";
                                toolTip += "Name: " + AddNameLines(item1) + "\n";
                                toolTip += "Start: " + addStartOfLine(item1) + "\n";
                                toolTip += "End: " + addEndOfLine(item1) + "\n";
                            }

                            line1.ToolTip = toolTip;
                            line3.ToolTip = toolTip;
                            line1.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line3.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line1.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            line3.MouseLeftButtonDown += Linija_MouseRightButtonDown;

                            ContextMenu menu = new ContextMenu();

                            foreach (var item1 in Lines3[i, j].listId)
                            {

                                MenuItem m1 = new MenuItem();
                                m1.Header = item1;
                                m1.Click += Find;
                                menu.Items.Add(m1);
                            }

                            line1.ContextMenu = menu;
                            line3.ContextMenu = menu;

                            Canvass.Children.Add(line1);
                            Canvass.Children.Add(line3);
                        }
                        else if (Lines3[i, j].Paint == positionLook.upsidedownL)
                        {
                            string toolTip = "";
                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                toolTip += "Id: " + item1.ToString() + "\n";
                                toolTip += "Name: " + AddNameLines(item1) + "\n";
                                toolTip += "Start: " + addStartOfLine(item1) + "\n";
                                toolTip += "End: " + addEndOfLine(item1) + "\n";
                            }
                            line2.ToolTip = toolTip;
                            line4.ToolTip = toolTip;
                            line2.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line4.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line2.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            line4.MouseLeftButtonDown += Linija_MouseRightButtonDown;

                            ContextMenu menu = new ContextMenu();
                            foreach (var item in Lines3[i, j].listId)
                            {
                                MenuItem m1 = new MenuItem();
                                m1.Header = item;
                                m1.Click += Find;
                                menu.Items.Add(m1);
                            }

                            line2.ContextMenu = menu;
                            line4.ContextMenu = menu;
                            Canvass.Children.Add(line2);
                            Canvass.Children.Add(line4);
                        }
                        else if (Lines3[i, j].Paint == positionLook.totalRotL)
                        {
                            string toolTip = "";

                            foreach (var item in Lines3[i, j].listId)
                            {
                                toolTip += "Id: " + item.ToString() + "\n";
                                toolTip += "Name: " + AddNameLines(item) + "\n";
                                toolTip += "Start: " + addStartOfLine(item) + "\n";
                                toolTip += "End: " + addEndOfLine(item) + "\n";
                            }

                            line2.ToolTip = toolTip;
                            line3.ToolTip = toolTip;
                            line2.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line3.MouseRightButtonDown += Linija_MouseRightButtonDown;
                            line2.MouseLeftButtonDown += Linija_MouseRightButtonDown;
                            line3.MouseLeftButtonDown += Linija_MouseRightButtonDown;

                            ContextMenu menu = new ContextMenu();

                            foreach (var item1 in Lines3[i, j].listId)
                            {
                                MenuItem m1 = new MenuItem();
                                m1.Header = item1;
                                m1.Click += Find;
                                menu.Items.Add(m1);

                            }

                            line2.ContextMenu = menu;
                            line3.ContextMenu = menu;
                            Canvass.Children.Add(line2);
                            Canvass.Children.Add(line3);
                        }
                        else if (Lines3[i, j].Paint == positionLook.plus)
                        {
                            List1 = LineTooltip(Lines[i, j], List1);

                            Canvass.Children.Add(List1[0]);
                            Canvass.Children.Add(List1[1]);
                            Canvass.Children.Add(List1[2]);
                            Canvass.Children.Add(List1[3]);
                            Canvass.Children.Add(el1);
                        }
                        else if (Lines3[i, j].Paint == positionLook.Tposition)
                        {
                            List1 = LineTooltip(Lines[i, j], List1);

                            Canvass.Children.Add(List1[1]);
                            Canvass.Children.Add(List1[2]);
                            Canvass.Children.Add(List1[3]);

                            Canvass.Children.Add(el1);
                        }
                        else if (Lines3[i, j].Paint == positionLook.upsidedownT)
                        {
                            List1 = LineTooltip(Lines[i, j], List1);

                            Canvass.Children.Add(List1[0]);
                            Canvass.Children.Add(List1[2]);
                            Canvass.Children.Add(List1[3]);

                            Canvass.Children.Add(el1);
                        }
                        else if (Lines3[i, j].Paint == positionLook.clockwise90T)
                        {
                            List1 = LineTooltip(Lines[i, j], List1);

                            Canvass.Children.Add(List1[0]);
                            Canvass.Children.Add(List1[1]);
                            Canvass.Children.Add(List1[2]);

                            Canvass.Children.Add(el1);
                        }
                        else if (Lines3[i, j].Paint == positionLook.notclockwise90T)
                        {
                            List1 = LineTooltip(Lines[i, j], List1);

                            Canvass.Children.Add(List1[0]);
                            Canvass.Children.Add(List1[1]);
                            Canvass.Children.Add(List1[3]);

                            Canvass.Children.Add(el1);
                        }
                    }
                }
            }
        }
        public void Find(object sender, System.EventArgs e)
        {
            long name = long.Parse(((MenuItem)sender).Header.ToString());
            if (el1 != null && el2 != null)
            {
                el1.Fill = br1;
                el2.Fill = br2;
            }
            foreach (var item in lineEntities)
            {
                if (item.Id == name)
                {
                    long end1 = item.FirstEnd;
                    long end2 = item.SecondEnd;

                    foreach (var item1 in ellipses)
                    {
                        if (!item1.Name.Contains("ee"))
                        {
                            if (("e_" + end1.ToString()) == item1.Name)
                            {
                                br1 = item1.Fill;

                                item1.Fill = Brushes.MistyRose;
                                el1 = item1;
                            }
                            if (("e_" + end2.ToString()) == item1.Name)
                            {
                                br2 = item1.Fill;
                                item1.Fill = Brushes.MistyRose;
                                el2 = item1;
                            }
                        }
                        else
                        {
                            string[] names = item1.Name.Split('_');

                            foreach (var item2 in names)
                            {
                                if (end1.ToString() == item2)
                                {
                                    br1 = item1.Fill;
                                    item1.Fill = Brushes.MistyRose;
                                    el1 = item1;
                                }
                                if (end2.ToString() == item2)
                                {
                                    br2 = item1.Fill;
                                    item1.Fill = Brushes.MistyRose;
                                    el2 = item1;
                                }
                            }
                        }
                    }
                }
            }
        }
        public List<Line> LineTooltip(MatrixElement param, List<Line> list)
        {
            foreach (var item in list)
            {
                item.ToolTip = "";
            }

            List<ContextMenu> menu = new List<ContextMenu>();

            foreach (var item in list)
            {
                menu.Add(new ContextMenu());
            }

            if (Lines2[param.Id.Item1, param.Id.Item2].Paint == positionLook.horizontally)
            {
                list[3].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[3].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";
                list[2].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[2].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";


                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                m2.Click += Find;

                menu[2].Items.Add(m1);
                menu[3].Items.Add(m2);
            }
            else if (Lines2[param.Id.Item1, param.Id.Item2].Paint == positionLook.straight)
            {
                list[0].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[0].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";
                list[1].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[1].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m2.Click += Find;

                menu[0].Items.Add(m1);
                menu[1].Items.Add(m2);
            }
            else if (Lines2[param.Id.Item1, param.Id.Item2].Paint == positionLook.Lposition)
            {
                list[0].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[0].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";
                list[3].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[3].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m2.Click += Find;

                menu[0].Items.Add(m1);
                menu[3].Items.Add(m2);
            }
            else if (Lines2[param.Id.Item1, param.Id.Item2].Paint == positionLook.othersideL)
            {
                list[0].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[0].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";
                list[2].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[2].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m2.Click += Find;

                menu[0].Items.Add(m1);
                menu[2].Items.Add(m2);
            }
            else if (Lines2[param.Id.Item1, param.Id.Item2].Paint == positionLook.upsidedownL)
            {
                list[1].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[1].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";
                list[3].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[3].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m2.Click += Find;

                menu[1].Items.Add(m1);
                menu[3].Items.Add(m2);
            }
            else if (Lines2[param.Id.Item1, param.Id.Item2].Paint == positionLook.totalRotL)
            {
                list[1].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[1].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";
                list[2].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString() + "\n";
                list[2].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[1]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[1].ToString();
                m2.Click += Find;

                menu[1].Items.Add(m1);
                menu[2].Items.Add(m2);
            }

            if (Lines3[param.Id.Item1, param.Id.Item2].Paint1 == positionLook.horizontally)
            {
                list[3].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[3].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";
                list[2].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[2].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m2.Click += Find;

                menu[2].Items.Add(m1);
                menu[3].Items.Add(m2);
            }
            else if (Lines3[param.Id.Item1, param.Id.Item2].Paint1 == positionLook.straight)
            {
                list[0].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[0].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";
                list[1].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[1].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m2.Click += Find;

                menu[0].Items.Add(m1);
                menu[1].Items.Add(m2);
            }
            else if (Lines3[param.Id.Item1, param.Id.Item2].Paint1 == positionLook.Lposition)
            {
                list[0].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[0].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";
                list[3].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[3].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m2.Click += Find;

                menu[0].Items.Add(m1);
                menu[3].Items.Add(m2);
            }
            else if (Lines3[param.Id.Item1, param.Id.Item2].Paint1 == positionLook.othersideL)
            {
                list[0].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[0].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";
                list[2].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[2].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m2.Click += Find;

                menu[2].Items.Add(m1);
                menu[0].Items.Add(m2);
            }
            else if (Lines3[param.Id.Item1, param.Id.Item2].Paint1 == positionLook.upsidedownL)
            {
                list[1].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[1].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";
                list[3].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[3].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m2.Click += Find;

                menu[1].Items.Add(m1);
                menu[3].Items.Add(m2);
            }
            else if (Lines3[param.Id.Item1, param.Id.Item2].Paint1 == positionLook.totalRotL)
            {
                list[1].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[1].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";
                list[2].ToolTip += Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString() + "\n";
                list[2].ToolTip += "Name: " + AddNameLines(Lines3[param.Id.Item1, param.Id.Item2].listId[0]) + "\n";

                MenuItem m1 = new MenuItem();
                m1.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m1.Click += Find;

                MenuItem m2 = new MenuItem();
                m2.Header = Lines3[param.Id.Item1, param.Id.Item2].listId[0].ToString();
                m2.Click += Find;

                menu[2].Items.Add(m1);
                menu[1].Items.Add(m2);
            }

            for (int i = 0; i < 4; i++)
            {
                list[i].ContextMenu = menu[i];
            }

            return list;


        }
        public string AddNameLines(long id)
        {

            string LineName = "";
            foreach (var item in lineEntities)
            {
                if (item.Id == id)
                {
                    LineName = item.Name;
                    break;
                }
            }
            return LineName;
        }
        public string addStartOfLine(long id)
        {

            string lineStart = "";
            foreach (var item in lineEntities)
            {
                if (item.Id == id)
                {
                    lineStart = item.FirstEnd.ToString();
                    break;
                }
            }
            return lineStart;
        }
        public string addEndOfLine(long id)
        {

            string lineEnd = "";
            foreach (var item in lineEntities)
            {
                if (item.Id == id)
                {
                    lineEnd = item.SecondEnd.ToString();
                    break;
                }
            }
            return lineEnd;
        }
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
        #endregion

        #region DRUGI DIO

        BrushConverter brushConverter = new BrushConverter();

        bool ellipse = false;
        bool polygon = false;
        bool addText = false;
        
        List<bool> funkcije = new List<bool>();
        List<System.Windows.Point> points = new List<System.Windows.Point>();

        public static Ellipse ellipseObj;
        public static Polygon polygonObj = new Polygon();

        public static bool leftClick = false;
        public static bool chngd = false;

        List<UIElement> element = new List<UIElement>();
        private bool clear = false;
        public static TextBlock block;
       
        private void Canvass_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            if (funkcije[0])
            {
                for (int i = 0; i < funkcije.Count; i++)
                {
                    if (funkcije[i] != false)
                    {
                        System.Windows.Point newPoint = e.GetPosition(this);
                        points.Add(newPoint);
                        leftClick = true;
                    }
                }
                EllipseWindow ellipseWindow = new EllipseWindow();
                ellipseWindow.ShowDialog();

                if (!EllipseWindow.Closed)
                {
                    ellipseObj = new Ellipse();
                    ellipseObj.Width = EllipseWindow.x;
                    ellipseObj.Height = EllipseWindow.y;
                    ellipseObj.StrokeThickness = EllipseWindow.borderThickness;
                    ellipseObj.Stroke = EllipseWindow.borderColor;
                    ellipseObj.Fill = EllipseWindow.fillColor;
                    ellipseObj.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                    ellipseObj.Opacity = EllipseWindow.opacity;
                    Canvas.SetTop(ellipseObj, points[0].Y - this.Menu.ActualHeight);
                    Canvas.SetLeft(ellipseObj, points[0].X);
                    Canvass.Children.Add(ellipseObj);
                }
                points.Clear();
                ResetAllColors();
                ResetAllBools();
            }

            else if(funkcije[1])
            {
                if (!PolygonWindow.Closed)
                {
                    System.Windows.Point newPoint = e.GetPosition(this);
                    polygonObj.Points.Add(newPoint);                    
                }
            }
            else if(funkcije[2])
            {
                for (int i = 0; i < funkcije.Count; i++)
                {
                    if (funkcije[i] != false)
                    {
                        System.Windows.Point newPoint = e.GetPosition(this);
                        points.Add(newPoint);
                        leftClick = true;
                    }
                }
                AddTextWindow addTextWindow = new AddTextWindow();
                addTextWindow.ShowDialog();

                if(!AddTextWindow.Closed)
                {
                    block = new TextBlock();
                    block.Text = AddTextWindow.text;
                    block.FontSize = AddTextWindow.textSize;
                    block.Foreground = AddTextWindow.textColor;
                    block.MouseLeftButtonDown += AddText_MouseLeftButtonDown;

                    Canvas.SetTop(block, points[0].Y);
                    Canvas.SetLeft(block, points[0].X);
                    Canvass.Children.Add(block);
                }
            }
            leftClick = true;
        }
        private void Canvass_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (funkcije[1])
            {
                PolygonWindow polygonWindow = new PolygonWindow();
                polygonWindow.ShowDialog();

                if (!PolygonWindow.Closed)
                {
                    //polygonObj = new Polygon();
                    polygonObj.StrokeThickness = PolygonWindow.borderThick;
                    polygonObj.Stroke = PolygonWindow.borderColor;
                    polygonObj.Fill = PolygonWindow.fillColor;
                    //polygonObj.Points = new PointCollection();
                    polygonObj.MouseLeftButtonDown += Polygon_MouseLeftButtonDown;

                    Canvass.Children.Add(polygonObj);
                }
                points.Clear();
                ResetAllColors();
                ResetAllBools();
                polygonObj = new Polygon();
                polygonObj.Points = new PointCollection();
            }
        }

        private void ResetAllColors()
        {
            foreach (MenuItem item in this.Menu.Items)
            {
                item.Background = Brushes.DeepPink;
                item.Foreground = Brushes.MintCream;
            }
        }

        private void ResetAllBools()
        {
            for (int i = 0; i < funkcije.Count; i++)
            {
                funkcije[i] = false;
            }
        }

        private void DrawEllipse_Click(object sender, RoutedEventArgs e)
        {
            ResetAllBools();
            ResetAllColors();
            points.Clear();
            EllipseWindow.Closed = false;
            this.DrawEllipse.Background = Brushes.MistyRose;
            this.DrawEllipse.Foreground = Brushes.Black;
            funkcije[0] = true;
        }

        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            ResetAllBools();
            ResetAllColors();
            points.Clear();
            PolygonWindow.Closed = false;
            this.DrawPolygon.Background = Brushes.MistyRose;
            this.DrawPolygon.Foreground = Brushes.Black;
            funkcije[1] = true;
        }
        
        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool pomocna = true;

            for(int i=0; i<funkcije.Count; i++)
            {
                if(funkcije[i])
                {
                    pomocna = false;
                }
            }
            if(pomocna)
            {
                ellipseObj = (Ellipse)sender;

                EllipseWindow ellipseWindow = new EllipseWindow();
                ellipseWindow.ShowDialog();

                ellipseObj.Fill = EllipseWindow.fillColor;
                ellipseObj.Stroke = EllipseWindow.borderColor;
                ellipseObj.Opacity = EllipseWindow.opacity;
                ellipseObj.StrokeThickness = EllipseWindow.borderThickness;
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (Canvass.Children.Count != 0)
            {
                element.Add(Canvass.Children[Canvass.Children.Count - 1]);
                Canvass.Children.Remove(Canvass.Children[Canvass.Children.Count - 1]);

            }
            else
            {
                if (element.Count != 0 && clear)
                {
                    for (int i = 0; i < element.Count; i++)
                    {
                        Canvass.Children.Add(element[i]);
                    }
                    element.Clear();
                    clear = false;
                }
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (element.Count != 0 && !clear)
            {
                Canvass.Children.Add(element[element.Count - 1]);
                element.Remove(element[element.Count - 1]);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (Canvass.Children.Count != 0)
            {
                clear = true;
                for (int i = 0; i < Canvass.Children.Count; i++)
                {
                    element.Add(Canvass.Children[i]);
                }
                Canvass.Children.Clear();
            }
        }

        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            ResetAllBools();
            ResetAllColors();
            points.Clear();
            AddTextWindow.Closed = false;
            this.AddText.Background = Brushes.MistyRose;
            this.AddText.Foreground = Brushes.Black;
            funkcije[2] = true;
        }

        private void Polygon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool allFalse = true;
            chngd = true;
            for (int i = 0; i < funkcije.Count; i++)
            {
                if (funkcije[i])
                    allFalse = false;
            }

            if (allFalse)
            {
                polygonObj = (Polygon)sender;

                PolygonWindow polyWind = new PolygonWindow();
                polyWind.ShowDialog();

                if (!PolygonWindow.Closed)
                {
                    polygonObj.Fill = PolygonWindow.fillColor;
                    polygonObj.Stroke = PolygonWindow.borderColor;
                    polygonObj.StrokeThickness = PolygonWindow.borderThick;
                }
            }
        }
        private void AddText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool allFalse = true;
            chngd = true;
            for (int i = 0; i < funkcije.Count; i++)
            {
                if (funkcije[i])
                    allFalse = false;
            }

            if (allFalse)
            {
                block = (TextBlock)sender;

                AddTextWindow polyWind = new AddTextWindow();
                polyWind.ShowDialog();

                if (!AddTextWindow.Closed)
                {
                    block.Text = AddTextWindow.text;
                    block.FontSize = AddTextWindow.textSize;
                    block.Foreground = AddTextWindow.textColor;
                }
            }
        }
        #endregion


        private void Linija_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            transformGroup.Children.Add(scaleTransform);
            DoubleAnimation myAnimation = new DoubleAnimation();
            myAnimation.From = 1;
            myAnimation.To = 2;
            myAnimation.AutoReverse = true;
            myAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

            Line linija = (Line)sender;
            string[] data = linija.ToolTip.ToString().Split('\n');
            var start = data[2].Replace(" ","").Split(':')[1];
            var end = data[3].Replace(" ", "").Split(':')[1];

            Ellipse newElipse1 = ellipses.Find(x => x.ToolTip.ToString().Contains(start));  //novi
            Ellipse newElipse2 = ellipses.Find(x => x.ToolTip.ToString().Contains(end));
            Ellipse oldElipse1 = ellipses.Find(x => x.ToolTip.ToString().Contains(helperA));  //stari
            Ellipse oldElipse2 = ellipses.Find(x => x.ToolTip.ToString().Contains(helperB));

            if (oldElipse1 != null)
            {
                oldElipse1.Fill = colorStart;
                oldElipse1.StrokeThickness = 1;
            }
            if (oldElipse2 != null)
            {
                oldElipse2.Fill = colorEnd;
                oldElipse2.StrokeThickness = 1;
            }

            if (newElipse1 != null && newElipse1.Fill != Brushes.Yellow && colorEnd != null) //odcekiraj kliknuo sam na isti koji je zut
            {
                colorStart = (SolidColorBrush)newElipse1.Fill;
                newElipse1.Fill = Brushes.Yellow;
                newElipse1.StrokeThickness = 2;
                newElipse1.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                newElipse1.RenderTransform = transformGroup;
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, myAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, myAnimation);
                helperA = start;
            }
            else
            {
                //colorStart = null;
                helperA = "////";
            }
            if (newElipse2 != null && newElipse2.Fill != Brushes.Yellow && colorStart != null) //odcekiraj kliknuo sam na isti koji je zut
            {
                colorEnd = (SolidColorBrush)newElipse2.Fill;
                newElipse2.Fill = Brushes.Yellow;
                newElipse2.StrokeThickness = 2;
                newElipse2.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                newElipse2.RenderTransform = transformGroup;
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, myAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, myAnimation);
                helperB = end;
            }
            else
            {
                //colorStart = null;
                helperB = "////";
            }



        }
    }
}
