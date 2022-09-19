using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Infrastructure.QnaMakerAI
{
    public class QnaMakerAIService : IQnaMakerAIService
    {
        public QnAMaker _qnaMakerResult { get; set; }

        public QnaMakerAIService(IConfiguration configuration)
        {
            _qnaMakerResult = new QnAMaker(new QnAMakerEndpoint
            {
                //leo del appsetting.json
                KnowledgeBaseId = configuration["QnaMakerBaseId"],
                EndpointKey = configuration["QnaMakerKey"],
                Host = configuration["QnaMakerHostName"]
            }) ;
        }
    }
}
