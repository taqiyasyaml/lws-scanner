using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CobaScanner
{
    internal class PreviewScannedImages
    {
        private class ImageMetadata
        {
            public int Acq;
            public int Page;
            public MemoryStream ImgStream;
            public ImageMetadata(int Acq, int Page, MemoryStream ImgStream)
            {
                this.Acq = Acq;
                this.Page = Page;
                this.ImgStream = ImgStream;
            }
        }

        private static List<ImageMetadata> ScannedImages = new List<ImageMetadata>();
        private static int IndexImage = -1;

        private Form1 form1;
        private PictureBox ScanImage;
        private Button NextImage;
        private Button PreviousImage;
        private Label ImageLabel;

        public PreviewScannedImages(Form1 form1, PictureBox scanImage, Button nextImage, Button previousImage, Label imageLabel)
        {
            this.form1 = form1;
            this.ScanImage = scanImage;
            this.NextImage = nextImage;
            this.PreviousImage = previousImage;
            this.ImageLabel = imageLabel;
            this.ClearImages();
        }

        public void ClearImages()
        {
            PreviewScannedImages.IndexImage = -1;
            this.form1.Invoke((MethodInvoker)delegate
            {
                if (this.ScanImage.Image != null)
                {
                    this.ScanImage.Image.Dispose();
                }
                this.ScanImage.Image = null;
                this.NextImage.Enabled = false;
                this.PreviousImage.Enabled = false;
                this.ImageLabel.Text = "";
            });
            for (int IImage = 0; IImage < PreviewScannedImages.ScannedImages.Count; IImage++)
            {
                PreviewScannedImages.ScannedImages[IImage].ImgStream.Dispose();
            }
            PreviewScannedImages.ScannedImages.Clear();
        }

        public void AddImage(int Acq, int Page, MemoryStream ImgStream)
        {
            PreviewScannedImages.ScannedImages.Add(new ImageMetadata(Acq, Page, ImgStream));
            if (PreviewScannedImages.ScannedImages.Count == 1)
            {
                PreviewScannedImages.IndexImage = 0;
                ImageMetadata TmpImg = PreviewScannedImages.ScannedImages[IndexImage];
                this.form1.Invoke((MethodInvoker)delegate
                {
                    if (this.ScanImage.Image != null)
                    {
                        this.ScanImage.Image.Dispose();
                        this.ScanImage.Image = null;
                    }
                    this.ScanImage.Image = new Bitmap(TmpImg.ImgStream);
                    this.NextImage.Enabled = false;
                    this.PreviousImage.Enabled = false;
                    this.ImageLabel.Text = "Acq " + (TmpImg.Acq+1).ToString() + " Page " + (TmpImg.Page+1).ToString();
                });
            }
            else if (PreviewScannedImages.ScannedImages.Count == 2)
            {
                this.form1.Invoke((MethodInvoker)delegate
                {
                    this.NextImage.Enabled = true;
                });
            }
        }

        public void NextImageOnClick()
        {
            if ((PreviewScannedImages.IndexImage + 1) < PreviewScannedImages.ScannedImages.Count)
            {
                PreviewScannedImages.IndexImage++;
                ImageMetadata TmpImg = PreviewScannedImages.ScannedImages[IndexImage];
                this.form1.Invoke((MethodInvoker)delegate
                {
                    if (this.ScanImage.Image != null)
                    {
                        this.ScanImage.Image.Dispose();
                        this.ScanImage.Image = null;
                    }
                    this.ScanImage.Image = new Bitmap(TmpImg.ImgStream);
                    this.NextImage.Enabled = (PreviewScannedImages.IndexImage + 1) < PreviewScannedImages.ScannedImages.Count;
                    this.PreviousImage.Enabled = (PreviewScannedImages.IndexImage - 1) >= 0;
                    this.ImageLabel.Text = "Acq " + (TmpImg.Acq + 1).ToString() + " Page " + (TmpImg.Page + 1).ToString();
                });
            }
            else
            {
                if (PreviewScannedImages.ScannedImages.Count == 0)
                {
                    this.ClearImages();
                }
                else if (PreviewScannedImages.ScannedImages.Count == 1)
                {
                    PreviewScannedImages.IndexImage = 0;
                    ImageMetadata TmpImg = PreviewScannedImages.ScannedImages[IndexImage];
                    this.form1.Invoke((MethodInvoker)delegate
                    {
                        if (this.ScanImage.Image != null)
                        {
                            this.ScanImage.Image.Dispose();
                            this.ScanImage.Image = null;
                        }
                        this.ScanImage.Image = new Bitmap(TmpImg.ImgStream);
                        this.NextImage.Enabled = false;
                        this.PreviousImage.Enabled = false;
                        this.ImageLabel.Text = "Acq " + (TmpImg.Acq + 1).ToString() + " Page " + (TmpImg.Page + 1).ToString();
                    });
                }
            }
        }

        public void PreviousImageOnClick()
        {
            if ((PreviewScannedImages.IndexImage - 1) >= 0)
            {
                PreviewScannedImages.IndexImage--;
                ImageMetadata TmpImg = PreviewScannedImages.ScannedImages[IndexImage];
                this.form1.Invoke((MethodInvoker)delegate
                {
                    if (this.ScanImage.Image != null)
                    {
                        this.ScanImage.Image.Dispose();
                        this.ScanImage.Image = null;
                    }
                    this.ScanImage.Image = new Bitmap(TmpImg.ImgStream);
                    this.NextImage.Enabled = (PreviewScannedImages.IndexImage + 1) < PreviewScannedImages.ScannedImages.Count;
                    this.PreviousImage.Enabled = (PreviewScannedImages.IndexImage - 1) >= 0;
                    this.ImageLabel.Text = "Acq " + (TmpImg.Acq + 1).ToString() + " Page " + (TmpImg.Page + 1).ToString();
                });
            }
            else
            {
                if (PreviewScannedImages.ScannedImages.Count == 0)
                {
                    this.ClearImages();
                }
                else if (PreviewScannedImages.ScannedImages.Count == 1)
                {
                    PreviewScannedImages.IndexImage = 0;
                    ImageMetadata TmpImg = PreviewScannedImages.ScannedImages[IndexImage];
                    this.form1.Invoke((MethodInvoker)delegate
                    {
                        if (this.ScanImage.Image != null)
                        {
                            this.ScanImage.Image.Dispose();
                            this.ScanImage.Image = null;
                        }
                        this.ScanImage.Image = new Bitmap(TmpImg.ImgStream);
                        this.NextImage.Enabled = false;
                        this.PreviousImage.Enabled = false;
                        this.ImageLabel.Text = "Acq " + (TmpImg.Acq + 1).ToString() + " Page " + (TmpImg.Page + 1).ToString();
                    });
                }
            }
        }

    }
}
