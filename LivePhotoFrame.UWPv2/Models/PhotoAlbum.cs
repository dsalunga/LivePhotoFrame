using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivePhotoFrame.UWP.Models
{
    public class PhotoRepository
    {
        public Guid Id { get; set; }
        public RepositoryConnection Connection { get; set; }

        public List<PhotoAlbum> Albums
        {
            get
            {
                return null;
            }
        }

        public PhotoRepository()
        {

        }
    }

    public class PhotoAlbum
    {
        public Guid Id { get; set; }

        public List<Photo> Photos
        {
            get
            {
                return null;
            }
        }

        public PhotoAlbum()
        {
        }
    }

    public class Photo
    {
        public Guid Id { get; set; }

        public string Path { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Guid AlbumId { get; set; }

        public PhotoAlbum Album
        {
            get
            {
                return null;
            }
        }

        public Photo()
        {

        }

        public bool IsPortrait()
        {
            return Height > Width;
        }
    }
}
