using BS_Core_WepApp.Key;
using BS_Core_WepApp.Services;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;
using BS_Core_WepApp.Models;
using Firebase.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using X.PagedList;

namespace BS_Core_WepApp.Controllers
{
    public class SetController : Controller
    {
        private IHostEnvironment _env;
        private IJavaScriptService _javascriptService;
        FirestoreDb firestoreDb;
        public SetController(IHostEnvironment env, IJavaScriptService javaScriptService)
        {
            _javascriptService = javaScriptService;
            _env = env;
            string keyPath = Path.Combine(_env.ContentRootPath, "Key\\newjoybayiapp-bb25d-firebase-adminsdk-gim0h-8d4f3dc2cb.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
            firestoreDb = FirestoreDb.Create(cls_keys.projectId);
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(SetModel setModel, int page = 1)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                Query setquery = firestoreDb.Collection("Set");
                QuerySnapshot documentSnapshots = await setquery.GetSnapshotAsync();
                List<SetModel> lstSet = new List<SetModel>();

                foreach (DocumentSnapshot item in documentSnapshots.Documents)
                {
                    if (item.Exists)
                    {
                        Dictionary<string, object> set = item.ToDictionary();
                        string jsonSet = JsonConvert.SerializeObject(set);
                        setModel = JsonConvert.DeserializeObject<SetModel>(jsonSet);

                        setModel.Id = item.Id.ToString();
                        lstSet.Add(setModel);
                    }
                }
                List<SetModel> sortSetList = lstSet.OrderBy(x => x.order).ToList();
                return View(sortSetList.ToPagedList(page, 10));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(SetModel setModel, string searchId, string searchGroupKod, string searchSetKod, string searchSetName, string submit)
        {

            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                Query setquery = firestoreDb.Collection("Set");
                QuerySnapshot documentSnapshots = await setquery.GetSnapshotAsync();
                List<SetModel> lstSet = new List<SetModel>();

                foreach (DocumentSnapshot item in documentSnapshots.Documents)
                {
                    if (item.Exists)
                    {
                        Dictionary<string, object> set = item.ToDictionary();
                        string jsonSet = JsonConvert.SerializeObject(set);
                        setModel = JsonConvert.DeserializeObject<SetModel>(jsonSet);

                        setModel.Id = item.Id.ToString();
                        lstSet.Add(setModel);
                    }
                }

                List<SetModel> sortSetList = null;

                if (submit == "Id" && searchId != null)
                {
                    sortSetList = lstSet.Where(x => x.Id.Contains(searchId)).OrderBy(x => x.order).ToList();
                }
                else if (submit == "GKod" && searchGroupKod != null)
                {
                    sortSetList = lstSet.Where(x => x.category.Contains(searchGroupKod)).OrderBy(x => x.order).ToList();
                }
                else if (submit == "SKod" && searchSetKod != null)
                {
                    sortSetList = lstSet.Where(x => x.code.Contains(searchSetKod)).OrderBy(x => x.order).ToList();
                }
                else if (submit == "Name" && searchSetName != null)
                {
                    sortSetList = lstSet.Where(x => x.name.Contains(searchSetName)).OrderBy(x => x.order).ToList();
                }
                else
                {
                    sortSetList = lstSet.OrderBy(x => x.order).ToList();
                }

                return View("Index", sortSetList.ToPagedList(1, 10));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SetModel setModel, IFormFile file0, IFormFile file1, IFormFile file2, IFormFile file3, IFormFile file4, IFormFile file5, IFormFile file6, IFormFile file7, IFormFile file8, IFormFile file9)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                if (ModelState.IsValid)
                {
                    string _strToken = HttpContext.Session.GetString("bt_userToken");
                    DocumentReference docRef = await firestoreDb.Collection("Set").AddAsync(setModel);

                    // Insert Storage
                    string link = null;
                    FileStream fs = null;
                    if (file0 != null && file0.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file0.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file0.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image0", link);
                    }

                    link = null;
                    fs = null;
                    if (file1 != null && file1.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file1.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file1.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image1", link);
                    }

                    link = null;
                    fs = null;
                    if (file2 != null && file2.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file2.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file2.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image2", link);
                    }

                    link = null;
                    fs = null;
                    if (file3 != null && file3.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file3.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file3.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image3", link);
                    }

                    link = null;
                    fs = null;
                    if (file4 != null && file4.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file4.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file4.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image4", link);
                    }

                    link = null;
                    fs = null;
                    if (file5 != null && file5.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file5.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file5.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image5", link);
                    }

                    link = null;
                    fs = null;
                    if (file6 != null && file6.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file6.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file6.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image6", link);
                    }

                    link = null;
                    fs = null;
                    if (file7 != null && file7.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file7.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file7.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image7", link);
                    }

                    link = null;
                    fs = null;
                    if (file8 != null && file8.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file8.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file8.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image8", link);
                    }

                    link = null;
                    fs = null;
                    if (file9 != null && file9.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file9.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file9.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Set")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image9", link);
                    }

                    return RedirectToAction("Index", "Set");
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Edit(string? id)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                DocumentReference docRef = firestoreDb.Collection("Set").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
                SetModel setModel = snapshot.ConvertTo<SetModel>();
                setModel.Id = snapshot.Id;
                return View(setModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SetModel setModel, IFormFile file0, IFormFile file1, IFormFile file2, IFormFile file3, IFormFile file4, IFormFile file5, IFormFile file6, IFormFile file7, IFormFile file8, IFormFile file9)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                if (ModelState.IsValid)
                {
                    string _strToken = HttpContext.Session.GetString("bt_userToken");
                    DocumentReference colref = firestoreDb.Collection("Set").Document(setModel.Id);
                    colref.SetAsync(setModel).GetAwaiter().GetResult();

                    string link = null;
                    FileStream fs = null;

                    string path = null;
                    string fileName = null;

                    if (file0 != null)
                    {
                        if (file0.Length > 0)
                        {
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file0.FileName}";

                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file0.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image0", link);
                        }
                    }

                    if (file1 != null)
                    {
                        if (file1.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file1.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file1.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image1", link);
                        }
                    }

                    if (file2 != null)
                    {
                        if (file2.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file2.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file2.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image2", link);
                        }
                    }

                    if (file3 != null)
                    {
                        if (file3.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file3.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file3.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image3", link);
                        }
                    }

                    if (file4 != null)
                    {
                        if (file4.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file4.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file4.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image4", link);
                        }
                    }

                    if (file5 != null)
                    {
                        if (file5.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file5.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file5.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image5", link);
                        }
                    }

                    if (file6 != null)
                    {
                        if (file6.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file6.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file6.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image6", link);
                        }
                    }

                    if (file7 != null)
                    {
                        if (file7.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file7.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file7.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image7", link);
                        }
                    }

                    if (file8 != null)
                    {
                        if (file8.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file8.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file8.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image8", link);
                        }
                    }

                    if (file9 != null)
                    {
                        if (file9.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file9.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file9.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Set")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image9", link);
                        }
                    }

                    return RedirectToAction("Index", "Set");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Delete(string? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                DocumentReference docRef = firestoreDb.Collection("Set").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
                SetModel setModel = snapshot.ConvertTo<SetModel>();
                setModel.Id = snapshot.Id;
                return View(setModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(SetModel setModel)
        {
            DocumentReference usrRef = firestoreDb.Collection("Set").Document(setModel.Id);
            usrRef.DeleteAsync().GetAwaiter().GetResult();
            return RedirectToAction("Index", "Set");
        }

        public IActionResult Details(string id)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                if (id == null)
                {
                    return BadRequest();
                }

                DocumentReference docRef = firestoreDb.Collection("Set").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
                if (snapshot.Exists)
                {
                    SetModel set = snapshot.ConvertTo<SetModel>();
                    set.Id = snapshot.Id;
                    return View(set);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
