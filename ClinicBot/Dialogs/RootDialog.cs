using ClinicBot.Common.Cards;
using ClinicBot.Data;
using ClinicBot.Dialogs.CreateAppoiment;
using ClinicBot.Dialogs.Qualification;
using ClinicBot.Infrastructure.Luis;
using ClinicBot.Infrastructure.QnaMakerAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        //variable local que invocara el servicio cognitivo de LUIS
        private readonly ILuisService _luisService;
        private IDataBaseService _dataBaseService;
        private readonly IQnaMakerAIService _qnaMakerAIService;
        public RootDialog(ILuisService luisService, IDataBaseService dataBaseService,UserState userState, IQnaMakerAIService qnaMakerAIService)
        {
            _qnaMakerAIService = qnaMakerAIService;
            _dataBaseService = dataBaseService;

            //hago la inyeccion del servicio cognitivo de Luis
            _luisService = luisService;

            //creo los pasos a seguir del flujo de los dialogos
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };

            //Agrego los dialogos que necesito utilizar
            AddDialog(new QualificationDialog(_dataBaseService));//este dialogo esta definido en la carpeta de Dialog/Qualification/
            AddDialog(new CreateAppoimentDialog(_dataBaseService,userState,_luisService));//este dialogo esta definido en la carpeta de Dialog/CreateAppoiment/
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            //agrego un id inicial al dialogo
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async  Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo el resultado de Luis
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(stepContext.Context,cancellationToken);
            
            //Administro las intenciones que obtenga de Luis
            return await ManageIntentions(stepContext, luisResult, cancellationToken);
        }

        private async Task<DialogTurnResult> ManageIntentions(WaterfallStepContext stepContext, Microsoft.Bot.Builder.RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //capturo la intencion con mayor probabildiad
            var topIntent = luisResult.GetTopScoringIntent();
            switch (topIntent.intent.ToLower())
            {
                case "saludar":
                    await IntentSaludar(stepContext, luisResult, cancellationToken);
                    break;
                case "agradecer":
                    await IntentAgradecer(stepContext, luisResult, cancellationToken);
                    break;
                case "despedir":
                    await IntentDespedir(stepContext, luisResult, cancellationToken);
                    break;
                case "veropciones":
                    await IntentVerOpciones(stepContext, luisResult, cancellationToken);
                    break;
                case "vercentrocontacto":
                    await IntentVerCentroContacto(stepContext, luisResult, cancellationToken);
                    break;
                case "calificar":
                    return await IntentCalificar(stepContext, luisResult, cancellationToken);
                case "crearcita":
                    return await IntentCrearCita(stepContext, luisResult, cancellationToken);
                case "vercita":
                     await IntentVerCita(stepContext, luisResult, cancellationToken);
                     break;
                case "none":
                    await IntentNone(stepContext, luisResult, cancellationToken);
                    break;
                default:
                    break;
            }

            //Indico que salte al siguiente metodo
            return await stepContext.NextAsync(cancellationToken:cancellationToken);
        }

        #region INTENT LUIS
        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Hola soy ClinicBOT que gusto verte", cancellationToken:cancellationToken);
            await Task.Delay(1000); //espera de 1 segundo

            //Muestro las Opciones
            await IntentVerOpciones(stepContext, luisResult, cancellationToken);
        }

        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No te preocupes me gusta ayudar", cancellationToken: cancellationToken);
        }

        private async  Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Espero verte pronto", cancellationToken: cancellationToken);
        }

        private async Task IntentVerOpciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Aqui tengo mis Opciones", cancellationToken: cancellationToken);

            //muestro la opciones 
            await MainOptionsCard.ToShow(stepContext,cancellationToken);
        }

        private async Task IntentVerCentroContacto(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            string phoneDetails = $"Nuestros numeros de atencion son los siguientes : {Environment.NewLine}" +
                $"📞 +57 321 711 5109 {Environment.NewLine}";

            string addresDetail = $"🏢 Estamos ubicandos en : {Environment.NewLine} Monteria-Cordoba Cl 23#8-71 Aguas Negras";

            await stepContext.Context.SendActivityAsync(phoneDetails, cancellationToken: cancellationToken);
            await Task.Delay(1000); //espera de 1 segundo
            await stepContext.Context.SendActivityAsync(addresDetail, cancellationToken: cancellationToken);
            await Task.Delay(1000); //espera de 1 segundo
            await stepContext.Context.SendActivityAsync("Te puedo ayudar en algo mas?", cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentCalificar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //indico que Inicie con el dialogo de Calificacion
            return await stepContext.BeginDialogAsync(nameof(QualificationDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentCrearCita(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //indico que Inicie con el dialogo de creacion de citas
            return await stepContext.BeginDialogAsync(nameof(CreateAppoimentDialog), cancellationToken: cancellationToken);
        }

        private async Task IntentVerCita(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Un momento por Favor...", cancellationToken: cancellationToken);
            await Task.Delay(1000);//espera de 1 segundo

            //capturo el id del usuario
            string idUser = stepContext.Context.Activity.From.Id;

            //consulto las citas medicas desde la bd
            var medicalData = _dataBaseService.MedicalAppoiment.Where(x => x.idUser == idUser).ToList();

            if (medicalData.Count > 0)
            {
                //obtengo las citas pendientes
                var pending = medicalData.Where(p => p.date >= DateTime.Now.Date).ToList();

                if (pending.Count > 0)
                {
                    await stepContext.Context.SendActivityAsync("Estas son tus citas Pendientes", cancellationToken: cancellationToken);

                    //recorro las citas pendientes
                    foreach (var item in pending)
                    {
                        await Task.Delay(1000);
                        //si la cita es es hoy pero ya se paso el tiempo continue con la siguiente cita
                        if (item.date == DateTime.Now.Date && item.time < DateTime.Now.Hour)
                        {
                            continue;
                        }
                           

                        string sumaryMedical = $"📅 Feha : {item.date.ToShortDateString()}" +
                            $"{Environment.NewLine} ⏰ Hora : {item.time}";

                        //muestro el mensaje con los datos de la cita
                        await stepContext.Context.SendActivityAsync(sumaryMedical, cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    // muestro el mensaje que no tiene citas pendientes
                    await stepContext.Context.SendActivityAsync("No tiene citas Pendientes", cancellationToken: cancellationToken);
                }
                   
            }
            else
            {
                // muestro el mensaje que no tiene citas pendientes
                await stepContext.Context.SendActivityAsync("No tiene citas Pendientes", cancellationToken: cancellationToken);
            }
            
        }

        private async Task IntentNone(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //valido para ver si esta intencion depronto esta en la bd de conocimientos de QnaMaker
            var resultQnA = await _qnaMakerAIService._qnaMakerResult.GetAnswersAsync(stepContext.Context);

            //capturo el nivel o porcentaje de confiabilidad la respuesta en qnaMker
            var score = resultQnA.FirstOrDefault()?.Score;
            //capturo la respuesta de QnaMaker
            string response = resultQnA.FirstOrDefault()?.Answer;

            //valido el nivel de confianza para dar una respuesta mas acertada
            if (score >= 0.5)
            {
                //Muestro la respuesta de QnaMaker
                await stepContext.Context.SendActivityAsync(response, cancellationToken:cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Disculpa no te entiendo lo que me dices", cancellationToken: cancellationToken);
                await Task.Delay(1000); //espera de 1 segundo

                //Muestro las Opciones
                await IntentVerOpciones(stepContext, luisResult, cancellationToken);

            }

            
        }
        #endregion

        private async  Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //indico que finaliza la conversacion o el dialogo
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        
    }
}
