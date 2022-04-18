using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public interface IMenuElement
    {
        public Brush Brush { get; set; } //Sima brusht használtam, mivel egy egyszerű szürke gomb, nincs textúrája
        public Point Position { get; set; }

        public bool IsClicked { get; set; } //Eltárolja, hogy rá lett-e kattintva az adott gombra

        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public string Command { get; set; } //Eltárolja, hogy minek a gombja (PL.: Start, Options, Exit)

    }
}
