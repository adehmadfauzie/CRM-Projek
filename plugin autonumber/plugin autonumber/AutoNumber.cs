using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace plugin_autonumber
{
    public class AutoNumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            try
            {

                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {

                    ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    if (context.MessageName.ToLower() != "create" && context.Stage == 20)
                    {
                        return;

                    }

                    Entity targetentity = context.InputParameters["Target"] as Entity;
                    Entity updateAutoNumberConfig = new Entity("app_app_autonumberconfiguration"); //entity custom untuk autonumber
                    StringBuilder autoNumber = new StringBuilder();
                    string prefix, suffix, separator, current, yeaar, month, day;
                    DateTime today = DateTime.Now;
                    day = today.Day.ToString("00");
                    month = today.Month.ToString("00");
                    yeaar = today.Year.ToString();
                    QueryExpression qeAutoNumberConfig = new QueryExpression()
                    {
                        EntityName = "app_app_autonumberconfiguration", //entity custom untuk autonumber
                        ColumnSet = new ColumnSet("app_prefix", "app_suffix", "app_separator", "app_currentnumber", "app_name")
                    };
                    EntityCollection ecAutoNumberConfig = service.RetrieveMultiple(qeAutoNumberConfig);
                    if (ecAutoNumberConfig.Entities.Count == 0)
                    {
                        return;
                    }

                    foreach (Entity entity in ecAutoNumberConfig.Entities)
                    {
                        if (entity.Attributes["app_name"].ToString().ToLower() == "applicationAutoNumber"); //field pada entity autonumber 
                        {
                            prefix = entity.GetAttributeValue<string>("app_prefix");
                            suffix = entity.GetAttributeValue<string>("app_suffix");
                            separator = entity.GetAttributeValue<string>("app_separator");
                            current = entity.GetAttributeValue<string>("app_currentnumber");
                            int tempCurrent = int.Parse(current);
                            tempCurrent++;
                            current = tempCurrent.ToString("000000");
                            updateAutoNumberConfig.Id = entity.Id;
                            updateAutoNumberConfig["app_currentnumber"] = tempCurrent.ToString();
                            service.Update(updateAutoNumberConfig);
                            autoNumber.Append(prefix + separator + yeaar + month + day + separator + suffix + current);
                            break;
                        }
                    }

                    targetentity["app_idrent"] = autoNumber.ToString(); //field target pada entity tujuan
                }
            }

            catch (Exception ex)
            {
                throw new InvalidCastException(ex.Message);
            }

               
            }
        }
    }
