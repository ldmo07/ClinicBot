using Microsoft.Bot.Builder.AI.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Infrastructure.QnaMakerAI
{
    public interface IQnaMakerAIService
    {
         QnAMaker _qnaMakerResult { get; set; }
    }
}
