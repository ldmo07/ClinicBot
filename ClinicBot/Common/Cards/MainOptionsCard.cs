using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Common.Cards
{
    public class MainOptionsCard
    {
        /// <summary>
        /// Metodo encargado de mostrar las opciones
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ToShow(DialogContext stepContext , CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: CreateCarousel(), cancellationToken: cancellationToken);
        }

        private static Activity CreateCarousel()
        {
            var cardCitasMedicas = new ThumbnailCard//HeroCard
            {
                Title ="Citas Medicas",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage ("https://clinicbotstorageluis.blob.core.windows.net/imagenes/historia-del-hospital.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction (){ Title ="Crear cita medica", Value = "Crear cita medica", Type = ActionTypes.ImBack},
                    new CardAction (){ Title ="Ver mi cita", Value = "Ver mi cita", Type = ActionTypes.ImBack},
                }
            };

            var cardInformacionContacto = new ThumbnailCard//HeroCard
            {
                Title = "Informacion Contacto",
                Subtitle = "Contacto",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorageluis.blob.core.windows.net/imagenes/directorio-telefonico.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction (){ Title ="Centro de contacto", Value = "Centro de contacto", Type = ActionTypes.ImBack},
                    new CardAction (){ Title ="Sitio Web", Value = "https://www.uniminuto.edu/", Type = ActionTypes.OpenUrl},
                }
            };

            var cardSiguenosRedes = new ThumbnailCard//HeroCard
            {
                Title = "Siguenos en la redes",
                Subtitle = "redes Sociales",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorageluis.blob.core.windows.net/imagenes/marketing-de-medios-sociales.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction (){ Title ="Facebook", Value = "https://www.uniminuto.edu/", Type = ActionTypes.OpenUrl},
                    new CardAction (){ Title ="Instagram", Value = "https://www.uniminuto.edu/", Type = ActionTypes.OpenUrl},
                }
               
            };

            var cardCalifiacion = new ThumbnailCard//HeroCard
            {
                Title = "Califiacion",
                Subtitle = "Calificanos",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorageluis.blob.core.windows.net/imagenes/satisfaccion.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction (){ Title ="Calificar Bot", Value = "Calificar bot", Type = ActionTypes.ImBack},
                }
            };

            //creo una lista de attachment con las trajetas
            var optionAttachments = new List<Attachment>()
            {
                cardCitasMedicas.ToAttachment(),
                cardInformacionContacto.ToAttachment(),
                cardSiguenosRedes.ToAttachment(),
                cardCalifiacion.ToAttachment(),
            };

            //añado la lista de attachment a un activity
            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}
