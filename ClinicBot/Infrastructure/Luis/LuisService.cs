﻿using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Infrastructure.Luis
{
    public class LuisService : ILuisService
    {
        public LuisRecognizer _luisRecognizer { get; set; }

        //paso como para  IConfiguration para poder llamar al appseting.json
        public LuisService(IConfiguration configuration)
        {
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],//lo leo del appseting.json
                configuration["LuisApiKey"],//lo leo del appseting.json
                configuration["LuisHostName"]//lo leo del appseting.json
            );

            //creo servicio luis de reconocimiento en la version3
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication) {

                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions()
                {
                    IncludeInstanceData = true
                }
            };

            //hago la inyeccion de LuisRecognizer
            _luisRecognizer = new LuisRecognizer(recognizerOptions);
        }
    }
}
