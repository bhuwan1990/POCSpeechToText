using Deepgram;
using Deepgram.Transcription;
using Microsoft.AspNetCore.Mvc;
using POCSpeechToText.Models;
using System.Diagnostics;

namespace POCSpeechToText.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //ViewBag.AudioText = await SpeechToTextAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Route("UploadFile")]
        public async Task<IActionResult> UploadFile()
        {
            var file = Request.Form.Files[0];

            // Check if the file was uploaded
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not selected");
            }
            

            // Save the file to disk
            var filePath = Path.Combine("Uploads", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

          var convertedText = await SpeechToTextAsync(filePath);


            return Ok(convertedText);
        }


        public async Task<string> SpeechToTextAsync(string filePath)
        {
            var credentials = new Credentials("c8500b5cb78a739de70d31ad61f25ada3b197198");

            string audioText = string.Empty;
            try
            {
                var deepgramClient = new DeepgramClient(credentials);

                using (FileStream fs = System.IO.File.OpenRead(filePath))
                {
                    PrerecordedTranscription response = await deepgramClient.Transcription.Prerecorded.GetTranscriptionAsync(
                        new Deepgram.Transcription.StreamSource(fs, "audio/wav"),
                        new Deepgram.Transcription.PrerecordedTranscriptionOptions()
                        {
                            Punctuate = true
                        });
                    audioText = response.Results.Channels.FirstOrDefault().Alternatives.FirstOrDefault().Transcript;


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }
            return audioText;
        }
    }
}