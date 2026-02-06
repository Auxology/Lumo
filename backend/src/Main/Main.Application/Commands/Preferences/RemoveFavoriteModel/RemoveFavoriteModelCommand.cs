using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Preferences.RemoveFavoriteModel;

public sealed record RemoveFavoriteModelCommand(string FavoriteModelId) : ICommand;