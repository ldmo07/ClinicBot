using ClinicBot.Common.Models.Qualification;
using ClinicBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Dialogs.Qualification
{
    public class QualificationDialog : ComponentDialog
    {
        private IDataBaseService _dataBaseService;
        public QualificationDialog(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            //creo los pasos a seguir en el flujo del dialogo
            var waterfallSteps = new WaterfallStep[]
            {
                ToShowButton,
                ValidateOption
            };

            //agrego el tipo de dialogos que voy a implementar
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> ToShowButton(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Muestro los botones de calificacion
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions
                {
                    //llamo al metod encargado de generar los botones de tipo SuggestAction
                    Prompt = CreateButtonsQualification()
                },
                cancellationToken
                );
        }

        private Activity CreateButtonsQualification()
        {
            //texto que se mostrara junto con los botones de califiacion
            var reply = MessageFactory.Text("Calificame por favor");

            //creo los botones suggesAction con sus Acciones
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "1🌟", Value = "1🌟", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "2🌟", Value = "2🌟", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "3🌟", Value = "3🌟", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "4🌟", Value = "4🌟", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "5🌟", Value = "5🌟", Type = ActionTypes.ImBack },
                }
            };

            //retorno la actividad
            return reply as Activity;
        }

        private async Task<DialogTurnResult> ValidateOption(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //capturo lo que el usuario ha seleccionado
            var options = stepContext.Context.Activity.Text;

            await stepContext.Context.SendActivityAsync($"Gracias por tu {options}",cancellationToken:cancellationToken);
            await Task.Delay(1000);//espera de 1 segundo

            await stepContext.Context.SendActivityAsync("¿En que mas te puedo ayudar?");

            //Mando a Guardar la calificacion en bd
            await SaveQualification(stepContext, options);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);//para que continue con el Dialogo
        }

        private async Task SaveQualification(WaterfallStepContext stepContext, string options)
        {
            //creo el modelo de la Qualificaction
            var qualificactionModel = new QualificationModel();
            qualificactionModel.id = Guid.NewGuid().ToString();
            qualificactionModel.idUser = stepContext.Context.Activity.From.Id;
            qualificactionModel.qualification = options;
            qualificactionModel.registerDate = DateTime.Now.Date;

            //invoco a la bd
            await _dataBaseService.Qualification.AddAsync(qualificactionModel); //paso el modelo a guardar
            await _dataBaseService.SaveAsync();//confirmo el commit
        }
    }
}
