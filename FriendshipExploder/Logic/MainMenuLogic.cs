using FriendshipExploder.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FriendshipExploder.Logic
{
    public class MainMenuLogic : IMainMenuModel, IMainMenuControl
    {
        
        


        public List<IMenuElement> MenuElements { get; set; }

        public MainMenuLogic()
        {
            MenuElements = new List<IMenuElement>();
            CreateButtons();
        }

       public void CreateButtons() //Feltölti a listát menü gomb elemekkel, szerintem lehet hardcodeolva átadni neki, mivel a menü gombok nem nagyon változnak
        {
            MenuElements.Add(new MenuButton("start",false, new Point(200, 200), new SolidColorBrush(Colors.Gray)));
            MenuElements.Add(new MenuButton("options",false, new Point(400, 200), new SolidColorBrush(Colors.Gray)));

        }

    }
}
