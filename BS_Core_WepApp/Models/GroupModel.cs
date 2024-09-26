using Google.Cloud.Firestore;
using System;

namespace BS_Core_WepApp.Models
{
    [FirestoreData]
    public class GroupModel : IDisposable
    {
        public string Id { get; set; }

        [FirestoreProperty]
        public string code { get; set; }

        [FirestoreProperty]
        public string name { get; set; }

        [FirestoreProperty]
        public int order { get; set; }

        [FirestoreProperty]
        public string image0 { get; set; }

        [FirestoreProperty]
        public string image1 { get; set; }

        [FirestoreProperty]
        public string image2 { get; set; }

        [FirestoreProperty]
        public string image3 { get; set; }

        [FirestoreProperty]
        public string image4 { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    
}
