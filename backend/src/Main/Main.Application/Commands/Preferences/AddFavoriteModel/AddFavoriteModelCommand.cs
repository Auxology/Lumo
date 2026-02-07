using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Preferences.AddFavoriteModel;

public sealed record AddFavoriteModelCommand(string ModelId) : ICommand<AddFavoriteModelResponse>;