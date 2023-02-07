using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Hubs
{
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class TaskNotificationHub:Hub
    {
        private readonly SchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly ILogger<TaskNotificationHub> _logger;
        private readonly MultiTenantService _multiTenantService;
        public TaskNotificationHub(SchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            MultiTenantService multiTenantService,
            ILogger<TaskNotificationHub> logger) {
            _schemaUpdateScriptBusiness = schemaUpdateScriptBusiness;
            _multiTenantService = multiTenantService;
            _logger = logger;
        }
        //重写连接事件，初次建立连接时进入此方法，开展具体业务可使用，例如管理连接池。
        public override async Task OnConnectedAsync() {
            await Clients.Caller.SendAsync("connected", Context.ConnectionId);
        }
        //重写断开事件，同理。
        public override async Task OnDisconnectedAsync(Exception exception) {
            await base.OnDisconnectedAsync(exception);
        }
        //服务端接收客户端发送方法
        public async Task SendMessage(string message) {
            //第一个参数为客户端接收服务端返回方法，名称需要服务端一致。
            await Clients.Caller.SendAsync("schemaUpdate", Context.ConnectionId + ":  " + message);
        }

        public async Task SchemaUpdate(Int64 scriptId) {

            Action<NoficationDto> action = async (dto) =>await sendMessage("schemaUpdate", dto);

            await sendMessage("schemaUpdate", new NoficationDto() {
                scriptId = scriptId,
                notifyLevel = (int)NotifyLevelEnum.Info,
                notifyTitle = $"开始执行",
            });

            var resultTuple= await _multiTenantService.ExecuteSchemaUpdate(scriptId, action);

            if(!resultTuple.Item1 && resultTuple.Item2==0) {
                await sendMessage("schemaUpdate", new NoficationDto() {
                    scriptId = scriptId,
                    notifyLevel = (int)NotifyLevelEnum.Error,
                    notifyTitle = "异常结束",
                    notifyBody = resultTuple.Item4
                });
                return;
            }

            await sendMessage("schemaUpdate",new NoficationDto() { 
                scriptId=scriptId,
                notifyLevel=(int)(NotifyLevelEnum.Info),
                notifyTitle="结束",
                notifyBody=$"成功：{resultTuple.Item2}，失败：{resultTuple.Item3}"
            });
        }

        private async Task sendMessage(string method, NoficationDto dto) {
           await Clients.Caller.SendAsync(method, dto);
        }

        public async Task SendAllMessage(string message) {
            //第一个参数为客户端接收服务端返回方法，名称需要服务端一致。
            await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId + ":  " + message);
        }
    }
}
