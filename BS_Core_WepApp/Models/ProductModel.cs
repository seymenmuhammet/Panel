using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace BS_Core_WepApp.Models
{
    [FirestoreData]
    public class ProductModel : IDisposable
    {
        public string Id { get; set; }

        [FirestoreProperty]
        public string erpCode { get; set; }

        [FirestoreProperty]
        public List<String> category0 { get; set; }

        [FirestoreProperty]
        public List<String> category1 { get; set; }

        [FirestoreProperty]
        public bool includeSet { get; set; }

        [FirestoreProperty]
        public string name { get; set; }

        [FirestoreProperty]
        public float price { get; set; }

        [FirestoreProperty]
        public float discountedPrice { get; set; }
        [FirestoreProperty]
        public float discountRate { get; set; }

        [FirestoreProperty]
        public float installmentPrice { get; set; }
        [FirestoreProperty]
        public float discountedInstallmentPrice { get; set; }

        [FirestoreProperty]
        public float discountInstallmentRate { get; set; }

        [FirestoreProperty]
        public int order { get; set; }

        [FirestoreProperty]
        public string description { get; set; }

        [FirestoreProperty]
        public string olc_der { get; set; }

        [FirestoreProperty]
        public string olc_gen { get; set; }

        [FirestoreProperty]
        public string olc_yuk { get; set; }

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

        [FirestoreProperty]
        public string image5 { get; set; }

        [FirestoreProperty]
        public string image6 { get; set; }

        [FirestoreProperty]
        public string image7 { get; set; }

        [FirestoreProperty]
        public string image8 { get; set; }

        [FirestoreProperty]
        public string image9 { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public string ShortDescription
        {
            get
            {
                if (description.Length > 20)
                {
                    return description.Substring(0, 20) + "...";
                }
                return description;
            }
        }
    }
}