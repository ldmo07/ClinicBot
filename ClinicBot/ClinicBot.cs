// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ClinicBot.Common.Models.User;
using ClinicBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot
{
    public class ClinicBot<T> : ActivityHandler where T : Dialog
    {
        #region VARIABLES
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly IDataBaseService _dataBaseService;
        #endregion

        #region CONSTRUCTOR
        public ClinicBot(UserState userState, ConversationState conversationState, T dialog, IDataBaseService dataBaseService)
        {
            //hago la inyeccion de dependencias 
            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            _dataBaseService = dataBaseService;
        }
        #endregion
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido soy ClinicBOT tu bot de servicios Medicos!"), cancellationToken);
                }
            }
        }

        //ESTE METODO CAPTURA LAS ACTIVIDADES TANTO DEL USUARIO COMO DEL BOT
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            //guardo los estados del usuario y los de la conversacion cuando ocurra un cambio
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        //ESTE METODO CAPTURA TODAS LAS ACTIVIDADES DEL USUARIO
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            /*
            //capturo lo que el usuario escribe
            var userMessage = turnContext.Activity.Text;
            //respondo al usuario
            await turnContext.SendActivityAsync($"Hola escribistes {userMessage}",cancellationToken:cancellationToken);
            */

            //capturo a los usuarios y los guardo en la bd de cosmos db en azure
            await SaveUser(turnContext);

            //hago la redireccion al dialogo
            await _dialog.RunAsync(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken);

        }

        /// <summary>
        /// METODO ENCARGADO DE SETEAR VALORES AL MODELO USUARIO Y MANDARLO A GUARDAR A LA BD
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        private async Task SaveUser(ITurnContext<IMessageActivity> turnContext)
        {
            //creo un nuveo modelo para enviarlo a la bd
            var userModel = new UserModel();
            userModel.id = turnContext.Activity.From.Id; //id unico para cada usuario en cada conversacion
            userModel.userNameChannel = turnContext.Activity.From.Name; //nombre del usuario por el canal
            userModel.channel = turnContext.Activity.ChannelId; //nombre del canal
            userModel.registerDate = DateTime.Now.Date;

            //mando a crear la bd en caso de que no exista
           /* using (var context = new DataBaseService())
            {
                //await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }*/

            //@@@ OJO MUY IMPORTANTE SI TE DA ERROR AL MOMENTO DE CREAR LA BD DESDE NET CORE ENTONCES CREALA DESDE AZURE @@@@
            //verifico si el usuario no existe lo mando a guardar
            var user = await _dataBaseService.User.FirstOrDefaultAsync(x => x.id == turnContext.Activity.From.Id);
            
            if(user == null)
            {
                //preparo el usuario para el guardado y confirmo la insercion o el commit
                await _dataBaseService.User.AddAsync(userModel);
                await _dataBaseService.SaveAsync();
            }
        }
    }
}
