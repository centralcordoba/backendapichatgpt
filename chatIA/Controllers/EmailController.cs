using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI.API;
using OpenAI.API.Completions;
using OpenAI.API.Models;
using System.Reflection;

namespace chatIA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {


        [HttpPost]
        [Route("actionsitems")]
        public IActionResult GetResult([FromBody] EmailBodyRequest prompt)
        {
            //your OpenAI API key
            string apiKey = "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";
            string actionItems = "Extraer los action items de este texto y darmelos enumerados";
            string answer = string.Empty;
            var openai = new OpenAIAPI(apiKey);
            CompletionRequest completion = new CompletionRequest();
            completion.Prompt = actionItems + prompt.EmailBody.ToString();
            completion.Model = Model.DavinciText;
            completion.MaxTokens = 4000;
            var result = openai.Completions.CreateCompletionAsync(completion);
            if (result != null)
            {
                foreach (var item in result.Result.Completions)
                {
                    answer = item.Text;
                }

                // Verificar si el texto es spam
                bool isSpam = IsSpam(answer);

                if (!isSpam)
                {


                    var response = new { answer };
                    string json = JsonConvert.SerializeObject(response);
                    return Content(json, "application/json");
                    
                }
                else
                {
                    return BadRequest("El texto es considerado como spam.");
                }
            }
            else
            {
                return BadRequest("No se encontró respuesta.");
            }
        }

        private bool IsSpam(string text)
        {

            string[] spamKeywords = { "ganar dinero", "oferta especial", "comprar ahora", "promoción exclusiva" };

            foreach (var keyword in spamKeywords)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }



        [HttpGet(Name = "getEmail")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {

            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(GmailService.Scope.GmailReadonly);
                }

                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "gmailCredenciales"
                });

                var emailListRequest = service.Users.Messages.List("me");
                emailListRequest.MaxResults = 10;
                var emailListResponse = await emailListRequest.ExecuteAsync();
                foreach (var email in emailListResponse.Messages)
                {
                    var emailDetailsRequest = service.Users.Messages.Get("me", email.Id);
                    var emailDetailsResponse = await emailDetailsRequest.ExecuteAsync();
                    // Process the email details
                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
            return (IEnumerable<WeatherForecast>)Ok();

        }


       
       
    }
}
