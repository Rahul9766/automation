using Microsoft.AspNetCore.Mvc;
using AzureServerManager.Services;
using AzureServerManager.Models;

namespace AzureServerManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly ServerService _serverService;
        private readonly FileOperationService _fileOperationService;

        public ServiceController(ServerService serverService, FileOperationService fileOperationService)
        {
            _serverService = serverService;
            _fileOperationService = fileOperationService;
        }

        [HttpGet("servers")]
        public IActionResult GetServers()
        {
            return Ok(_serverService.GetServers());
        }

        [HttpGet("indexing-servers")]
        public IActionResult GetIndexingServers()
        {
            return Ok(_serverService.GetIndexingServers());
        }

        [HttpGet("thinclient-servers")]
        public IActionResult GetThinClientServers()
        {
            return Ok(_fileOperationService.GetThinClientServers());
        }

        [HttpPost("control-service")]
        public async Task<IActionResult> ControlService([FromBody] ServiceOperationRequest request)
        {
            var result = await _serverService.ControlService(request);
            return Ok(new { result });
        }

        [HttpPost("control-all-indexing")]
        public async Task<IActionResult> ControlAllIndexingServices([FromBody] string operation)
        {
            var result = await _serverService.ControlAllIndexingServices(operation);
            return Ok(new { result });
        }

        [HttpPost("file-operation")]
        public async Task<IActionResult> PerformFileOperation([FromBody] FileOperationRequest request)
        {
            var result = await _fileOperationService.PerformFileOperation(request);
            return Ok(new { result });
        }

        [HttpPost("file-operation-all")]
        public async Task<IActionResult> PerformFileOperationOnAllServers([FromBody] dynamic request)
        {
            string operation = request.operation;
            string folderPath = request.folderPath;
            
            var result = await _fileOperationService.PerformFileOperationOnAllServers(operation, folderPath);
            return Ok(new { result });
        }
    }
} 