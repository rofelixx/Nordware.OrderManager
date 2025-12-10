using Microsoft.AspNetCore.Mvc;
using OrderManager.Application.Dto;

namespace OrderManager.Web.Api.Controllers
{
    [ApiController]
    [Route("mocks/freight")]
    public class MockFreightController : ControllerBase
    {
        [HttpPost("quote")]
        public ActionResult<FreightQuoteResponseDto> Quote([FromBody] FreightQuoteRequestDto req)
        {
            var price = Math.Round(10 + req.WeightKg * 2 + req.VolumeM3 * 50, 2);
            return Ok(new FreightQuoteResponseDto { Price = price, EstimatedDays = 3 });
        }
    }
}
