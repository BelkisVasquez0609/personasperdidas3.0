using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Net;
using System.Net.Mail;
using System.Windows;
using VideoFrameAnalyzer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Forms.VisualStyles.VisualStyleElement.ComboBox;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using FaceAPI = Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using VisionAPI = Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.SharePoint.Client;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Media;

namespace faceapp
{
   public partial class SimuladorImagen : System.Windows.Forms.Form
    {
        // Variables
        Image img2;
        Image img1;
        static double porcent;
        string fileUri2;
        BitmapImage bit2;
        static string porcentaje;
        IFaceClient clientFace;
        const string RECOGNITION_MODEL1 = RecognitionModel.Recognition02;
        GeoCoordinateWatcher watcher;
        bool accesox = true;
        string filePath1;
        BitmapImage bit1;
        private bool ExistenDispositivos = false;
        private FilterInfoCollection DispositivosDeVideo;
        private VideoCaptureDevice FuenteDeVideo = null;
        private IList<DetectedFace> faceList;
        private double resizeFactor;
        private string[] faceDescriptions;
        string defaultStatusBarText = "";

        //Constructor
        public SimuladorImagen(IFaceClient x)
        {
         
           
        InitializeComponent();
        clientFace = x;
        watcher = new GeoCoordinateWatcher();
        BuscarDispositivos();

        }     
        //Metodos
        public void SendCorreos(string correo2, string asunto, string cuerpo)
        {
        var fromAddress = new MailAddress("belvasquez06@gmail.com", "Vecindario Seguro");
        var toAddress = new MailAddress(correo2, correo2);
        string fromPassword = "abcd123@A";
        string subject = asunto;
        string body = cuerpo;
        var smtp = new SmtpClient
        {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };
        using (var message = new MailMessage(fromAddress, toAddress)
        {
        Subject = subject,
        Body = body
        })
        {
        smtp.Send(message);
        }
        }
        public byte[] Imglist(int id)
        {

        var x = new DataSet1TableAdapters.DesaparecidosTableAdapter();
        var y = x.ListaImagenes(id);
        return (byte[]) y;
        }
        public byte[] ImageToByteArray(System.Drawing.Image imagen)
        {
        MemoryStream ms = new MemoryStream();
        imagen.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        return ms.ToArray();
        }
        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
        MemoryStream ms = new MemoryStream(byteArrayIn);
        return Image.FromStream(ms);
        }
        private async void button1_Click(object sender, EventArgs e)
        {
        try
        {
        var x = new DataSet1TableAdapters.DesaparecidosTableAdapter();
        var z = new DataSet1TableAdapters.RegistroSimilitudTableAdapter();
        var y = x.GetDataBy3();
        img1 = pictureBox2.Image;
        
        for (int i = 0; i < y.Count; i++)
        {
        if (Imglist(y[i].id) != null)
        {
        img2 = ByteArrayToImage(Imglist(y[i].id));

        await FindSimilar(clientFace, imagen(img1), imagen(img2), RECOGNITION_MODEL1);
        await DetectarRostro(bit1, filePath1);
        if (porcent > 55)
        {
        MessageBox.Show("Esta Imagen es parecida con un " + porcent + "%" + " con el desaparecido: " + x.GetNombreDesaparecido(y[i].id) + " Con el Correo: " + x.GetCorreoDesaparecida(y[i].id));
        z.InsertQueryRegistroSimilutud(ImageToByteArray(img1), y[i].id, DateTime.Now, GetLocationProperty(), Convert.ToDouble(porcent));
        SendCorreos(x.GetCorreoDesaparecida(y[i].id).ToString(), "Encontramos tu desaparecido", "Existe una hay Imagen que es parecida a tu desaparecido: "+x.GetNombreDesaparecido(y[i].id) +" con un: " + porcent + "% de similitud" + " y las siguiente descripcion: " + "\n \n" + defaultStatusBarText + ". \n \n En la siguiente ubicacion: " + GetLocationProperty()  +"\n \n" + " Verifique su aplicacion y use el codigo de verificacion: " + z.GetUltimoID());
        break;
        }

        }
        }
        if (porcent <= 55)
        {
        MessageBox.Show("No se Encontraron Similitudes con ningun desaparecido");
        }

        

        button1.Visible = false;
        button2.Visible = true;
        }
        catch (Exception)
        {

                
        }
        }
        public Stream imagen(Image img)
        {
        var stream = new System.IO.MemoryStream();
        img.Save(stream, ImageFormat.Jpeg);
        stream.Position = 0;
        return stream;
        }
        private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, Stream img, string RECOGNITION_MODEL1)
        {
        IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(img, recognitionModel: RECOGNITION_MODEL1);

        return detectedFaces.ToList();
        }
        public static async Task FindSimilar(IFaceClient client, Stream img1, Stream img2, string RECOGNITION_MODEL1)
        {

        IList<Guid?> targetFaceIds = new List<Guid?>();

        var faces = await DetectFaceRecognize(client, img2, RECOGNITION_MODEL1);
        targetFaceIds.Add(faces[0].FaceId.Value);

        IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client, img1, RECOGNITION_MODEL1);

        IList<SimilarFace> similarResults = await client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null, null, targetFaceIds);

        foreach (var similarResult in similarResults)
        {
        porcentaje = $"{similarResult.Confidence * 100}" + "%";
        porcent = similarResult.Confidence * 100;
        }


        }   
        private void button2_Click(object sender, EventArgs e)
        {
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
        //Get the path of specified file
         filePath1 = openFileDialog1.FileName;

        var fileUri2 = new Uri(filePath1);
        BitmapImage bitmapSource = new BitmapImage();

        bitmapSource.BeginInit();
        bitmapSource.CacheOption = BitmapCacheOption.None;
        bitmapSource.UriSource = fileUri2;
        bitmapSource.EndInit();
        bit1 = bitmapSource;
        pictureBox2.Image = new Bitmap(filePath1);

        button1.Visible = true;
        button2.Visible = false;
        }
        }
        public string GetLocationProperty()
        {
          
        // Do not suppress prompt, and wait 1000 milliseconds to start.
        watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            
        GeoCoordinate coord = watcher.Position.Location;

        if (coord.IsUnknown != true)
        {

        return " Latitude: "+ coord.Latitude+", " +"Longitude: "  + coord.Longitude;
        }
        else
        {
        return "Unknown latitude and longitude.";
        }
        }
        private void button3_Click(object sender, EventArgs e)
        {
        MessageBox.Show(GetLocationProperty());
        }
        private void btnCerrar_Click(object sender, EventArgs e)
        {
        TerminarFuenteDeVideo();
        this.Close();
        }     
        public void CargarDispositivos()
        {
        List<string> numbers = new List<string>();
        for (int i = 0; i < DispositivosDeVideo.Count; i++)
        { 
           
        string hola = DispositivosDeVideo[i].Name.ToString();

        numbers.Add(hola);

        }
        camarasbox.Items.Clear();
        camarasbox.Items.AddRange(numbers.ToArray());
        camarasbox.SelectedIndex = 0;
        }

        public void BuscarDispositivos()
        {
        DispositivosDeVideo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        if (DispositivosDeVideo.Count == 0)
        ExistenDispositivos = false;
        else
        {
        ExistenDispositivos = true;
        CargarDispositivos();
        //   iniciar.Enabled = true;
        }
        }

        public void TerminarFuenteDeVideo()
        {
        if (!(FuenteDeVideo == null))
        if (FuenteDeVideo.IsRunning)
        {
        FuenteDeVideo.SignalToStop();
        FuenteDeVideo = null;
        }
        }

        private void video_NuevoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap Imagen = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = Imagen;

        }
        private  void button3_Click_1(object sender, EventArgs e)
        {
        FuenteDeVideo = new VideoCaptureDevice(DispositivosDeVideo[camarasbox.SelectedIndex].MonikerString);
        FuenteDeVideo.NewFrame += new NewFrameEventHandler(video_NuevoFrame);
        FuenteDeVideo.Start();
        button4.Visible = true;
        }

        private void camarasbox_SelectedIndexChanged(object sender, EventArgs e)
        {
        TerminarFuenteDeVideo();
            button4.Visible = false;
        }

        private void SimuladorImagen_FormClosed(object sender, FormClosedEventArgs e)
        {
        TerminarFuenteDeVideo();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
          

            //BitmapImage bitmapSource = new BitmapImage();
            //filePath1 = pictureBox1.ImageLocation;
            //var fileUri1 = new Uri(filePath1);
            //bitmapSource.BeginInit();
            //bitmapSource.CacheOption = BitmapCacheOption.None;
            //bitmapSource.UriSource = fileUri1;
            //bitmapSource.EndInit();

            //bit1 = bitmapSource;

            //await DetectarRostro(bit1, filePath1);
            await ValidarImagenEnVivo();
        }

        private async Task DetectarRostroEnVivo()
        {
        try
        {
        accesox = false;

        IList<DetectedFace> detectedFaces = await DetectFaceRecognize(clientFace, imagen(pictureBox1.Image), RECOGNITION_MODEL1);
        if (detectedFaces.Count() > 0)
        {
        MessageBox.Show("Rostro Identificado");
        accesox = false;
        }
        else
        {
        accesox = true;
        }
        }
        catch (Exception)
        {

               
        }
           
        }

        private async Task ValidarImagenEnVivo()
        {
        try
        {
        var x = new DataSet1TableAdapters.DesaparecidosTableAdapter();
        var z = new DataSet1TableAdapters.RegistroSimilitudTableAdapter();
        var y = x.GetDataBy3();
        img1 = pictureBox1.Image;

        for (int i = 0; i < y.Count; i++)
        {
        if (Imglist(y[i].id) != null)
        {
        img2 = ByteArrayToImage(Imglist(y[i].id));

        await FindSimilar(clientFace, imagen(img1), imagen(img2), RECOGNITION_MODEL1);

        if (porcent > 55)
        {
        MessageBox.Show("Esta Imagen es parecida con un " + porcent + "%" + " con el desaparecido: " + x.GetNombreDesaparecido(y[i].id) + " Con las Siguientes Caracteristicas: "+defaultStatusBarText+" Dicha informacion sera enviada al correo: "+ x.GetCorreoDesaparecida(y[i].id));
        //  z.InsertQueryRegistroSimilutud(ImageToByteArray(img1), y[i].id, DateTime.Now, GetLocationProperty(), Convert.ToDouble(porcent));
        // SendCorreos(x.GetCorreoDesaparecida(y[i].id).ToString(), "Encontramos tu desaparecido", "hay Imagen es parecida con un " + porcent + "% de similitud" + " con el desaparecido: " + x.GetNombreDesaparecido(y[i].id) + ",\n en la siguiente ubicacion " + GetLocationProperty() + " Verifique su aplicacion y use el codigo de verificacion: " + z.GetUltimoID());

        break;
        }

        }
        }
        if (porcent <= 55)
        {
        MessageBox.Show("No se Encontraron Similitudes con ningun desaparecido");
        }



        button1.Visible = false;
        button2.Visible = true;
        }
        catch (Exception)
        {


        }
        }
      
        private async Task DetectarRostro(BitmapImage bitmapSource,string path)
        {
        try
        {


        faceList = await UploadAndDetectFaces(path);

        if (faceList.Count > 0)
        {
        // Prepare to draw rectangles around the faces.
        DrawingVisual visual = new DrawingVisual();
        DrawingContext drawingContext = visual.RenderOpen();
        drawingContext.DrawImage(bitmapSource,
        new System.Windows.Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
        double dpi = bitmapSource.DpiX;
        // Some images don't contain dpi info.
        resizeFactor = (dpi == 0) ? 1 : 96 / dpi;
        faceDescriptions = new String[faceList.Count];

        for (int i = 0; i < faceList.Count; ++i)
        {
        DetectedFace face = faceList[i];

        // Draw a rectangle on the face.
        drawingContext.DrawRectangle(
        System.Windows.Media.Brushes.Transparent,
        new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 2),
        new System.Windows.Rect(
        face.FaceRectangle.Left * resizeFactor,
        face.FaceRectangle.Top * resizeFactor,
        face.FaceRectangle.Width * resizeFactor,
        face.FaceRectangle.Height * resizeFactor
        )
        );

        // Store the face description.
        faceDescriptions[i] = FaceDescription(face);
        defaultStatusBarText = FaceDescription(face);
        }

        drawingContext.Close();

        // Display the image with the rectangle around the face.
        RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
        (int)(bitmapSource.PixelWidth * resizeFactor),
        (int)(bitmapSource.PixelHeight * resizeFactor),
        96,
        96,
        PixelFormats.Pbgra32);

        faceWithRectBitmap.Render(visual);
        //  img = faceWithRectBitmap;

        // Set the status bar text.
       // MessageBox.Show(defaultStatusBarText);
        }
        }
        catch (Exception)
        {


        }
        }

        private async Task<IList<DetectedFace>> UploadAndDetectFaces(string imageFilePath)
        {
        try
        {
        // The list of Face attributes to return.
        IList<FaceAttributeType> faceAttributes =
        new FaceAttributeType[]
        {
        FaceAttributeType.Gender, FaceAttributeType.Age,
        FaceAttributeType.Smile, FaceAttributeType.Emotion,
        FaceAttributeType.Glasses, FaceAttributeType.Hair
        };

        // Call the Face API.

        using (Stream imageFileStream = System.IO.File.OpenRead(imageFilePath))
        {
        // The second argument specifies to return the faceId, while
        // the third argument specifies not to return face landmarks.
        IList<DetectedFace> faceList =
        await clientFace.Face.DetectWithStreamAsync(
        imageFileStream, true, false, faceAttributes);
        return faceList;
        }
        }
        // Catch and display Face API errors.
        catch (APIErrorException f)
        {
        MessageBox.Show(f.Message);
        return new List<DetectedFace>();
        }
        // Catch and display all other errors.
        catch (Exception e)
        {
        MessageBox.Show(e.Message, "Error");
        return new List<DetectedFace>();
        }
        }

        private string FaceDescription(DetectedFace face)
        {
        try
        {


        StringBuilder sb = new StringBuilder();

        sb.Append("Face: ");

        // Add the gender, age, and smile.
        sb.Append(face.FaceAttributes.Gender);
        sb.Append(", ");
        sb.Append(face.FaceAttributes.Age);
        sb.Append(", ");
        sb.Append(String.Format("smile {0:F1}%, ", face.FaceAttributes.Smile * 100));

        // Add the emotions. Display all emotions over 10%.
        sb.Append("Emotion: ");
        Emotion emotionScores = face.FaceAttributes.Emotion;
        if (emotionScores.Anger >= 0.1f) sb.Append(
        String.Format("anger {0:F1}%, ", emotionScores.Anger * 100));
        if (emotionScores.Contempt >= 0.1f) sb.Append(
        String.Format("contempt {0:F1}%, ", emotionScores.Contempt * 100));
        if (emotionScores.Disgust >= 0.1f) sb.Append(
        String.Format("disgust {0:F1}%, ", emotionScores.Disgust * 100));
        if (emotionScores.Fear >= 0.1f) sb.Append(
        String.Format("fear {0:F1}%, ", emotionScores.Fear * 100));
        if (emotionScores.Happiness >= 0.1f) sb.Append(
        String.Format("happiness {0:F1}%, ", emotionScores.Happiness * 100));
        if (emotionScores.Neutral >= 0.1f) sb.Append(
        String.Format("neutral {0:F1}%, ", emotionScores.Neutral * 100));
        if (emotionScores.Sadness >= 0.1f) sb.Append(
        String.Format("sadness {0:F1}%, ", emotionScores.Sadness * 100));
        if (emotionScores.Surprise >= 0.1f) sb.Append(
        String.Format("surprise {0:F1}%, ", emotionScores.Surprise * 100));

        // Add glasses.
        sb.Append(face.FaceAttributes.Glasses);
        sb.Append(", ");

        // Add hair.
        sb.Append("Hair: ");

        // Display baldness confidence if over 1%.
        if (face.FaceAttributes.Hair.Bald >= 0.01f)
        sb.Append(String.Format("bald {0:F1}% ", face.FaceAttributes.Hair.Bald * 100));

        // Display all hair color attributes over 10%.
        IList<HairColor> hairColors = face.FaceAttributes.Hair.HairColor;
        foreach (HairColor hairColor in hairColors)
        {
        if (hairColor.Confidence >= 0.1f)
        {
        sb.Append(hairColor.Color.ToString());
        sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
        }
        }

        // Return the built string.
        return sb.ToString();
        }
        catch (Exception)
        {

        return null;
        }
        }

    }
}
