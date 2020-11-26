using System.Collections.Generic;
using CommandAPI.Data;
using CommandAPI.Models;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using CommandAPI.Dtos;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;

namespace CommandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        //random change
        private readonly ICommandAPIRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandAPIRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        //Add the following code
        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands()
        {
            var commandItems = _repository.GetAllCommands();
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        //Add the following code for our second ActionResult
        //Name="GetCommandById" labels the method as "GetCommandById" for the nameof operator in the POST request
        [Authorize]
        [HttpGet("{id}", Name = "GetCommandById")]
        public ActionResult<CommandReadDto> GetCommandById(int id)
        {
            var commandItem = _repository.GetCommandById(id);
            if (commandItem == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CommandReadDto>(commandItem));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        {
            // convert to received external facing type (DTO) to internal type (Hidden model)
            var commandModel = _mapper.Map<Command>(commandCreateDto);
            // add the internal model instance to the DbContext (local changes)
            _repository.CreateCommand(commandModel);
            // save the local changes in the repository (DbContext) to the database
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);
            // make a call to the Get By ID route and return the newly created object.
            // params: the name of the route method to call, the url parameters, the external type returned from the Get By ID request.
            // the DTO returned from this CreatedAtRoute method contains the Id that the internal type was created with.
            return CreatedAtRoute(nameof(GetCommandById), new { Id = commandReadDto.Id }, commandReadDto);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCommand(int id, CommandUpdateDto commandUpdateDto)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if (commandModelFromRepo == null)
            {
                return NotFound();
            }
            // this mapping here does the actual update of the DbContext
            _mapper.Map(commandUpdateDto, commandModelFromRepo);
            // this call to the repo does nothing but it exists for uniformity purposes?
            _repository.UpdateCommand(commandModelFromRepo);
            // save the changes from the DbContext to the database.
            _repository.SaveChanges();
            return NoContent();
        }

        [HttpPatch("{id}")]
        // the method receives the url parameter id and a JsonPatchDocument that maps to a CommandUpdateDto. 
        public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
        {
            // retrieve the commandModel from the repo
            var commandModelFromRepo = _repository.GetCommandById(id);
            if (commandModelFromRepo == null)
            {
                return NotFound();
            }

            // create an update dto from the repo object
            var commandToPatch = _mapper.Map<CommandUpdateDto>(commandModelFromRepo);
            // apply the JsonPatchDocument to the Update DTO
            patchDoc.ApplyTo(commandToPatch, ModelState);
            // validate the update dto instance against the model
            if (!TryValidateModel(commandToPatch))
            {
                return ValidationProblem(ModelState);
            }
            // recreate the command instance using the patched DTO
            _mapper.Map(commandToPatch, commandModelFromRepo);
            // blank update command (for future database changes)
            _repository.UpdateCommand(commandModelFromRepo);
            // save the dbcontext rows to the database.
            _repository.SaveChanges();
            // return HTTP Code 204
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteCommand(int id)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if (commandModelFromRepo == null)
            {
                return NotFound();
            }
            _repository.DeleteCommand(commandModelFromRepo);
            _repository.SaveChanges();
            return NoContent();
        }

        // [HttpGet]
        // public ActionResult<IEnumerable<string>> Get()
        // {
        //     return new string[] {"this", "is", "hard", "coded"};
        // }
    }
}