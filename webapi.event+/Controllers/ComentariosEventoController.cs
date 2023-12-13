using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapi.event_.Domains;
using webapi.event_.Repositories;
using webapi.event_.Interfaces;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using System.Text;

namespace webapi.event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentariosEventoController : ControllerBase
    {


        // acesso aos métodos do repositório 
        ComentariosEventoRepository comentario = new ComentariosEventoRepository();

        //  Armazena dados da API externa (IA - Azure)
        private readonly ContentModeratorClient _contentModeratorClient;

        public ComentariosEventoController(ContentModeratorClient contentModeratorClient)
        {
            _contentModeratorClient = contentModeratorClient;
        }

        [HttpPost("ComentarioIA")]
        public async Task<IActionResult> PostIA(ComentariosEvento comentariosEvento)
        {
            try
            {
                // se a descrição do comentário não for passado no objeto 
                if (string.IsNullOrEmpty(comentariosEvento.Descricao))
                {
                    return BadRequest("O Texto a ser analisado não pode ser Vazio !!");
                }
                // converte a string(descrição do comentário em um MemoryStream
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(comentariosEvento.Descricao));

                // realiza a moderação do conteúdo(descrição do comentário) 
                var moderationResult = await _contentModeratorClient.TextModeration
                    .ScreenTextAsync("text/plain", stream, "por", false, false, null, true);

                // se existir termos ofensivos
                if (moderationResult.Terms != null)
                {
                    //atribuir false para "Exibe" e também cadastra o Comentário
                    comentariosEvento.Exibe = false;

                    comentario.Cadastrar(comentariosEvento);
                }
                else
                {
                    comentariosEvento.Exibe = true;

                    comentario.Cadastrar(comentariosEvento);
                }
                return StatusCode(201, comentariosEvento);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }


        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(comentario.Listar());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("ListarSomenteExibe")]
        public IActionResult GetIA()
        {
            try
            {
                return Ok(comentario.ListarSomenteExibe());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }




        [HttpGet("BuscarPorIdUsuario/{id}")]
        public IActionResult GetByIdUser(Guid idUsuario, Guid idEvento)
        {
            try
            {
                return Ok(comentario.BuscarPorIdUsuario(idUsuario, idEvento));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public IActionResult Post(ComentariosEvento novoComentario)
        {
            try
            {
                comentario.Cadastrar(novoComentario);
                return StatusCode(201, novoComentario);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                comentario.Deletar(id);
                return NoContent();
            }

            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }
}
