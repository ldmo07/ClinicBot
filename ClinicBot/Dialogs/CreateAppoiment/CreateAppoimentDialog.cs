using ClinicBot.Common.Models.User;
using ClinicBot.Common.Models.MedicalAppoiment;
using ClinicBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClinicBot.Common.Models.BotState;
using ClinicBot.Infrastructure.Luis;
using ClinicBot.Common.Models.EntityLuis;

namespace ClinicBot.Dialogs.CreateAppoiment
{
    public class CreateAppoimentDialog : ComponentDialog
    {
        private readonly IDataBaseService _dataBaseService;
        public static UserModel newUserModel = new UserModel();
        public static MedicalAppoimentModel medicalAppoimentModel = new MedicalAppoimentModel();
        private readonly IStatePropertyAccessor<BotStateModel> _userState;
        static string userText;
        private readonly ILuisService _luisService;

        public CreateAppoimentDialog(IDataBaseService dataBaseService, UserState userState, ILuisService luisService)
        {
            _luisService = luisService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));
            _dataBaseService = dataBaseService;

            //creo el flujo que seguira el dialogo
            var waterfallStep = new WaterfallStep[]
            {
                SetPhone,
                SetFullName,
                SetEmail,
                SetDate,
                SetTime,
                Confirmation,
                FinalProcess
            };

            //Agrego los Dialogos que voy a usar
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),waterfallStep));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> SetPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo el primer texto que ingresa el usuario en la accion crear cita
            userText = stepContext.Context.Activity.Text;

            //capturo o traigo el contexto de la conversacion para saber si el usuario ya ingreso su informacion  
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            //si ya el usuario tiene data me paso al siguiente metodo del flujo sino pido los datos
            if (userStateModel.medicalData)
            {
                return await stepContext.NextAsync(cancellationToken:cancellationToken);
            }
            else
            {
                //pido el numero de telefono
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Por favor Ingresa tu numero de Telefono :") },
                    cancellationToken
                );
            }
        }

        private async Task<DialogTurnResult> SetFullName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo o traigo el contexto de la conversacion para saber si el usuario ya ingreso su informacion
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            //si ya el usuario tiene data me paso al siguiente metodo del flujo sino pido los datos
            if (userStateModel.medicalData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                //capturo el numero de telefono
                var userPhone = stepContext.Context.Activity.Text;

                newUserModel.phone = userPhone; //lleno el telefono en el modelo

                //muestro un nuevo mensaje
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Ingresa tu nombre completo :") },
                    cancellationToken
                );
            }
        }

        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo o traigo el contexto de la conversacion para saber si el usuario ya ingreso su informacion
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            //si ya el usuario tiene data me paso al siguiente metodo del flujo sino pido los datos
            if (userStateModel.medicalData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                //capturo el nombre completo del usuario
                var fullNameuser = stepContext.Context.Activity.Text;

                newUserModel.fullName = fullNameuser; //lleno el fullName en el modelo

                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu email :") },
                    cancellationToken
                );
            }
           
        }

        private async Task<DialogTurnResult> SetDate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo el correo del usuario
            var userEmail = stepContext.Context.Activity.Text;

            newUserModel.email = userEmail; //lleno el email en el modelo

            //creo un nuevo contexto y le asigno la frase inical que el usuario escribio al momento de intentar agendar la cita
            var newStepContext = stepContext;
            newStepContext.Context.Activity.Text = userText;
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(newStepContext.Context,cancellationToken);

            //verifico si Luis a detectado alguna entidad y la mapeo al Modelo EntityLuisModel
            var Entity = luisResult.Entities.ToObject<EntityLuisModel>();

            //valido si existen entidades de tipo DateTime
            if (Entity.dateTime != null)
            {
                //extraigo la fecha y hago el replace de xxxx por el año actual
                var date = Entity.dateTime.First().timex.First().Replace("XXXX",DateTime.Now.Year.ToString());
                //la fecha puede ser devuelta asi  2020-01-02  o asi 2020-01-02T01

                //valido si la fecha viene en el formato 2020-01-02T01 que tome apena 10 caracteres
                if (date.Length > 10)
                {
                    date = date.Remove(10);
                }

                //asigno la fecha al modelo y le indico que se salte al siguiente metodo
                medicalAppoimentModel.date = DateTime.Parse(date);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);

            }
            else
            {
                string text = $"Ahora necesito la fecha en la que desea la cita Medica con el siguiente Formato dd/mm/yyyy";

                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text(text) }
                );
            } 
        }

        private async Task<DialogTurnResult> SetTime(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //valido que si el modelo no tiene una fecha la capture del context anterior y si ya la tiene que no la capture nuevamente
            if(medicalAppoimentModel.date == DateTime.MinValue)
            {
                //capturo la fecha
                var medicalDate = stepContext.Context.Activity.Text;
                medicalAppoimentModel.date = Convert.ToDateTime(medicalDate); //lleno la fecha en el modelo
            }

            return await stepContext.PromptAsync(
               nameof(TextPrompt),
               new PromptOptions { 
                   Prompt = CreateButtonsTime() //llamo metodo encrgado de mostrar Botones de tiempob
               },
               cancellationToken
            );
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo la hora 
            var medicalTime = stepContext.Context.Activity.Text;

            medicalAppoimentModel.time = int.Parse(medicalTime); //lleno la hora en el modelo
            
            return await stepContext.PromptAsync(
               nameof(TextPrompt),
               new PromptOptions
               {
                   Prompt = CreateButtonConfirmation() //llamo metodo encrgado de mostrar Botones de confirmacion
               },
               cancellationToken
            );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //obtengo la confirmacion del usuario
            var userConfirmation = stepContext.Context.Activity.Text;

            if (userConfirmation.ToLower().Equals("si"))
            {
                //mando a guardar la cita en la bd
                string userId = stepContext.Context.Activity.From.Id; //capturo el Id del usuario

                //valido que no esxista ese registro
                var userModel = await _dataBaseService.User.FirstOrDefaultAsync(x => x.id == userId);

                //capturo o traigo el contexto de la conversacion para saber si el usuario ya ingreso su informacion
                var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

                //valido si el usuario no tiene la data inicial lo actualizo en bd
                if (!userStateModel.medicalData)
                {
                    //Actualizo el Modelo
                    userModel.phone = newUserModel.phone;
                    userModel.fullName = newUserModel.fullName;
                    userModel.email = newUserModel.email;

                    //Actualizo en la bd
                    _dataBaseService.User.Update(userModel);
                    await _dataBaseService.SaveAsync();
                }
               
                //Guardo la cita Medica
                medicalAppoimentModel.id = Guid.NewGuid().ToString();
                medicalAppoimentModel.idUser = userId;
                await _dataBaseService.MedicalAppoiment.AddAsync(medicalAppoimentModel); //Agergo el modelo a guardar
                await _dataBaseService.SaveAsync(); //confirmo el commit o guardaro de la cita Medica

                //envio mensaje de confirmacion
                await stepContext.Context.SendActivityAsync("Tu cita se Agendo con exito", cancellationToken: cancellationToken);

                //indico que el usuario ya ingreso los datos inciales
                userStateModel.medicalData = true;

                //Muestro informacion de la cita Medica
                string summaryMedical = $"Para : {userModel.fullName}" +
                    $"{Environment.NewLine} 📞 Telefono : {userModel.phone}" +
                    $"{Environment.NewLine} ✉ Email : {userModel.email}" +
                    $"{Environment.NewLine} 📅 Fecha : {medicalAppoimentModel.date}" +
                    $"{Environment.NewLine} ⏰ Hora : {medicalAppoimentModel.time}";

                await stepContext.Context.SendActivityAsync(summaryMedical, cancellationToken: cancellationToken);
                await Task.Delay(1000); //espera de 1 segundo
                await stepContext.Context.SendActivityAsync("En que mas Puedo ayudarte?", cancellationToken: cancellationToken);
                medicalAppoimentModel = new MedicalAppoimentModel(); //inicializo el modelo nuevamente para que se limpie
            }
            else
            {
                //Muestro mensaje
                await stepContext.Context.SendActivityAsync("No hay Problema, Sera a la proxima",cancellationToken:cancellationToken);
            }

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken); //para que continue el dialogo
        }

        private Activity CreateButtonsTime()
        {
            var reply = MessageFactory.Text("Ahora Selecciona la Hora");

            //creo los botones de tipo SuggestAction
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title="9",  Value ="9", Type = ActionTypes.ImBack},
                    new CardAction(){Title="10", Value ="10", Type = ActionTypes.ImBack},
                    new CardAction(){Title="11", Value ="11", Type = ActionTypes.ImBack},
                    new CardAction(){Title="15", Value ="15", Type = ActionTypes.ImBack},
                    new CardAction(){Title="16", Value ="16", Type = ActionTypes.ImBack},
                    new CardAction(){Title="17", Value ="17", Type = ActionTypes.ImBack},
                    new CardAction(){Title="18", Value ="18", Type = ActionTypes.ImBack},
                }
            };

            return reply as Activity;
        }

        private Activity CreateButtonConfirmation()
        {
            var reply = MessageFactory.Text("Confirmas la creacion de esta cita medica?");

            //creo los botones de tipo SuggestAction
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title="Si",  Value ="Si", Type = ActionTypes.ImBack},
                    new CardAction(){Title="No", Value ="No", Type = ActionTypes.ImBack},
                }
            };

            return reply as Activity;
        }
    }
}
