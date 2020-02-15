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

		string _agentName = "cursos2-jaijhy";

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

						if (cursos!= null && cursos.ListValue.Values.Count > 0)
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

				else if (action == "ActionTesteWHPayload")
				{
					var contexto = request.QueryResult.OutputContexts;

					response.FulfillmentText = "Teste Payload no WH com sucess.";
					var payload = "{\"list\": {\"replacementKey\": \"@contexto\",\"invokeEvent\": true,\"afterDialog\": true,\"itemsName\": [\"Sim\",\"Não\"],\"itemsEventName\": [\"QueroInscrever\",\"NaoQueroInscrever\"]}}";

					response.FulfillmentMessages.Add(new Intent.Types.Message()
					{

						Payload = Google.Protobuf.WellKnownTypes.Struct.Parser.ParseJson(payload)

					});
					
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

		[HttpGet("[action]")]
		public ActionResult CriarEntidade()
		{
			//usa credencias

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();

			EntityType entidade = new EntityType();
			entidade.DisplayName = "Cursos";
			entidade.Kind = EntityType.Types.Kind.Map;

			DAL.CursoDAL dal = new DAL.CursoDAL();

			foreach (var curso in dal.ObterTodos())
			{
				var item = new EntityType.Types.Entity();
				item.Value = curso.Nome;

				foreach (var s in curso.Sinonimos)
				{
					item.Synonyms.Add(s);
				}

				entidade.Entities.Add(new EntityType.Types.Entity(item));
			}

			var request = new Google.Cloud.Dialogflow.V2.CreateEntityTypeRequest();
			request.EntityType = entidade;
			request.ParentAsProjectAgentName = new ProjectAgentName(_agentName);

			c.CreateEntityType(request);


			return Ok("Entidade criada.");

		}

		[HttpGet("[action]")]
		public ActionResult ExcluirEntidade(bool apenasItens = false)
		{
			//usa credencias

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();

			var list = c.ListEntityTypes(new ProjectAgentName(_agentName));

			foreach (var entidade in list)
			{

				if (entidade.DisplayName == "Cursos")
				{

					if (apenasItens)
					{
						c.BatchDeleteEntities(entidade.EntityTypeName,
							entidade.Entities.Select(e => e.Value).ToArray());
					}
					else {
						c.DeleteEntityType(entidade.EntityTypeName);
					}

					break;
				}
			}



			return Ok("Entidade excluída.");

		}


		[HttpGet("[action]")]
		public ActionResult AlterarEntidade()
		{
			//usa credencias

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();

			var list = c.ListEntityTypes(new ProjectAgentName(_agentName));

			foreach (var entidade in list)
			{

				if (entidade.DisplayName == "Cursos")
				{
					var item = entidade.Entities.Where(e => e.Value == "Sistemas de Informação").FirstOrDefault();

					if (item != null)
					{
						item.Synonyms.Add("BSI2");

						var request = new Google.Cloud.Dialogflow.V2.UpdateEntityTypeRequest();
						request.EntityType = entidade;
						c.UpdateEntityType(request);
					}

					break;
				}
			}



			return Ok("Entidade alterada.");

		}


		[HttpGet("[action]")]
		public ActionResult CriarIntencao()
		{
			Google.Cloud.Dialogflow.V2.IntentsClient c = IntentsClient.Create();

			Intent intent = new Intent();
			intent.DisplayName = "Curso.Nome";

			var frase1 = new Intent.Types.TrainingPhrase();
			frase1.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part()
			{
				Text = "Qual o valor do curso de "

			});

			frase1.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part()
			{
				Text = "sistemas de informação",
				EntityType = "Cursos"
			});


			var frase2 = new Intent.Types.TrainingPhrase();
			frase2.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part()
			{
				Text = "Qual o preço do curso de "

			});

			frase2.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part()
			{
				Text = "sistemas de informação",
				EntityType = "Cursos"
			});


			intent.TrainingPhrases.Add(frase1);
			intent.TrainingPhrases.Add(frase2);

			var resposta = new Intent.Types.Message();
			resposta.Text = new Intent.Types.Message.Types.Text();
			resposta.Text.Text_.Add("1000.00");

			intent.Messages.Add(resposta);

			var request = new Google.Cloud.Dialogflow.V2.CreateIntentRequest();
			request.Intent = intent;
			request.ParentAsProjectAgentName = new ProjectAgentName(_agentName);

			c.CreateIntent(request);

			return Ok("Entidade criada");
		}


	}
}
 