using AutoMapper;
using CommandAPI.Dtos;
using CommandAPI.Models;
namespace CommandAPI.Profiles
{
    // Profile here is from Automapper.Profile
    public class CommandsProfile : Profile
    {
        public CommandsProfile()
        {
            // internal -> external mapping (GET request retrieves data from the database)
            // source   -> target mapping
            CreateMap<Command, CommandReadDto>();
            // external -> internal (Post request sends data to the databse)
            CreateMap<CommandCreateDto, Command>();
            CreateMap<CommandUpdateDto, Command>();
            CreateMap<Command, CommandUpdateDto>();
        }
    }
}