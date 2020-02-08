using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebhookDF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
		private static readonly JsonParser _jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

		System.Text.Json.JsonSerializerOptions _jsonSetting = new System.Text.Json.JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true
		};

		public WebhookController()
		{
		}


		[HttpGet]
        public IActionResult Get()
        {
			return Ok(new { msg = "deu certo" });
        }

		private bool Autorizado(IHeaderDictionary httpHeader)
		{

			string basicAuth = httpHeader["Authorization"];

			if (!string.IsNullOrEmpty(basicAuth))
			{
				basicAuth = basicAuth.Replace("Basic ", "");

				byte[] aux = System.Convert.FromBase64String(basicAuth);
				basicAuth = System.Text.Encoding.UTF8.GetString(aux);

				if (basicAuth == "nome:token")
					return true;
			}

			return false;
		}
		
		[HttpPost("GetWebhookResponse")]
		public ActionResult GetWebhookResponse([FromBody] System.Text.Json.JsonElement dados)
		{
			if (!Autorizado(Request.Headers))
			{
				return StatusCode(401);
			}

			WebhookRequest request =
				_jsonParser.Parse<WebhookRequest>(dados.GetRawText());

			WebhookResponse response = new WebhookResponse();


			if (request != null)
			{

				string action = request.QueryResult.Action;
				var parameters = request.QueryResult.Parameters;

				if (action == "ActionTesteWH")
				{

					response.FulfillmentText = "testando o webhook 2";
				}
				else if (action == "ActionCursoOferta")
				{
					DAL.CursoDAL dal = new DAL.CursoDAL();

					if (parameters != null &&
						parameters.Fields.ContainsKey("Cursos"))
					{
						var cursos = parameters.Fields["Cursos"];

						if (cursos.StringValue != "")
						{
							string curso = cursos.ListValue.Values[0].StringValue;
							if (dal.ObterCurso(curso) != null)
							{
								response.FulfillmentText = "Sim, temos " + curso + ".";
							}
						}
						else
						{
							response.FulfillmentText = "Não temos, mas temos esses: " + dal.ObterTodosFormatoTexto() + ".";
						}
					}
				
				}
				else if (action == "ActionCursoValor")
				{
					var contexto = request.QueryResult.OutputContexts;

					if (contexto[0].ContextName.ContextId == "ctxcurso")
					{
						if (contexto[0].Parameters != null &&
						contexto[0].Parameters.Fields.ContainsKey("Cursos"))
						{
							var cursos = contexto[0].Parameters.Fields["Cursos"];
							string curso = cursos.ListValue.Values[0].StringValue;
							DAL.CursoDAL dal = new DAL.CursoDAL();

							Models.Curso c = dal.ObterCurso(curso);
							if (c != null)
							{
								response.FulfillmentText = 
									"A mensalidade para " + c.Nome + " é " + c.Preco + ".";
							}
						}
					}
				}
					

			}

			return Ok(response);


		}

		[HttpPost("GetWebhookResponse2")]
		public ActionResult GetWebhookResponse2([FromBody] System.Text.Json.JsonElement dados)
		{
			if (!Autorizado(Request.Headers))
			{
				return StatusCode(401);
			}

			WebhookRequest request =
				_jsonParser.Parse<WebhookRequest>(dados.GetRawText());

			WebhookResponse response = new WebhookResponse();


			if (request != null)
			{

				string action = request.QueryResult.Action;

				if (action == "ActionTesteWH")
				{

					response.FulfillmentText = "testando o webhook 2";
				}

			}

			return Ok(response);


		}


	}
}
 