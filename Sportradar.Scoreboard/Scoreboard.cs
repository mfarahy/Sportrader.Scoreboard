﻿using FluentResults;

namespace Sportradar.Scoreboard
{
    public class Scoreboard
    {
        public Scoreboard()
        {
            _onlineMatches = new HashSet<Match>();
        }

        private HashSet<Match> _onlineMatches;

        public IReadOnlyCollection<Match> OnlineMatches
        {
            get { return _onlineMatches; }
        }

        public Result<Match> CreateMatch(Team homeTeam, Team awayTeam)
        {
            if (homeTeam == null)
            {
                return Result.Fail($"Home team is null!");
            }
            if (awayTeam == null)
            {
                return Result.Fail($"Away team is null!");
            }
            if (_onlineMatches.Any(x => x.IsParticipated(homeTeam)))
            {
                return Result.Fail($"Currently ${homeTeam} is playing!");
            }
            if (_onlineMatches.Any(x => x.IsParticipated(awayTeam)))
            {
                return Result.Fail($"Currently ${awayTeam} is playing!");
            }
            if (homeTeam.Equals(awayTeam))
            {
                return Result.Fail($"A team can not play with itself!");
            }

            var match = new Match(homeTeam, awayTeam);
            match.OnCanceled += Match_OnCanceled;
            match.OnFinished += Match_OnFinished;

            var startResult = match.Start();
            if (!startResult.IsSuccess)
            {
                return startResult;
            }

            var updateResult = match.UpdateScore(0, 0);
            if (!updateResult.IsSuccess)
            {
                return updateResult;
            }

            _onlineMatches.Add(match);

            return Result.Ok(match);
        }

        private void Match_OnFinished(object? sender, CompletedMatchResult e)
        {
            var match = (Match)sender;

            _onlineMatches.Remove(match);
        }

        private void Match_OnCanceled(object? sender, CanceledMatchResult e)
        {
            var match = (Match)sender;

            _onlineMatches.Remove(match);
        }

        public ScoreboardSummary GetSummary()
        {
            return new ScoreboardSummary(_onlineMatches.OrderDescending(new MatchComparer()));
        }

        public void ClearBoard()
        {
            _onlineMatches.Clear();
        }
    }
}
