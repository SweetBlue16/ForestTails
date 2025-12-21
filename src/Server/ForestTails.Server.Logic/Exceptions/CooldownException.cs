using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class CooldownException : ForestTailsException
    {
        public CooldownException(string actionName, double secondsLeft)
        : base(MessageCode.SkillCooldown, $"'{actionName}' is in cool down. Please wait {secondsLeft:F1}s.")
        {}
    }
}
