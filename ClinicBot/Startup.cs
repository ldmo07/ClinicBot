// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.16.0

using ClinicBot.Data;
using ClinicBot.Dialogs;
using ClinicBot.Infrastructure.Luis;
using ClinicBot.Infrastructure.QnaMakerAI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClinicBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //CONFIGURACION DEL BLOBSTORAGE
            var storage = new BlobsStorage(
                //leo estos valores desde el appsetting.json
                Configuration.GetSection("StorageConnectionString").Value,
                Configuration.GetSection("StorageContainer").Value
               );

            //VARIABLE QUE MANEJARA EL ESTADO DEL USUARIO
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            //VARIABLE QUE MANEJARA EL ESTADO DE LA CONVERSACION
            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            //AGREGA EL CONTEXTO DE BD DE COSMOS
            services.AddDbContext<DataBaseService>(options =>
            {
                //indico que usare cosmos
                options.UseCosmos(
                    Configuration["CosmosEndPoint"], //lo saco del appseting.json
                    Configuration["CosmosKey"], //lo saco del appseting.json
                    Configuration["CosmosDatabase"] //lo saco del appseting.json
                );
            });

            //REGISTRO EL SERVICIO DE BD DE COSMOS
            services.AddScoped<IDataBaseService, DataBaseService>();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //Agrego interfaz de la clase LuisServices
            services.AddSingleton<ILuisService, LuisService>();

            //Agrego interfaz de QnaMaker
            services.AddSingleton<IQnaMakerAIService, QnaMakerAIService>();

            //Agrego el dialogo creado
            services.AddTransient<RootDialog>();//services.AddSingleton<RootDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, ClinicBot<RootDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
