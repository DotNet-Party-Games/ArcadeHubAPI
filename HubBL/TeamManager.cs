using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubDL;
using HubEntities.Database;

namespace HubBL {
    public class TeamManager {
        private readonly IDatabase<Team> _teamDB;
        private readonly IDatabase<User> _userDB;
        private readonly IDatabase<TeamJoinRequest> _joinRequestDB;
        private readonly IList<string> _teamIncludes;

        public TeamManager(IDatabase<Team> teamDB, IDatabase<TeamJoinRequest> joinRequestDB,
            IDatabase<User> userDB) {
            _teamDB = teamDB;
            _userDB = userDB;
            _joinRequestDB = joinRequestDB;

            _teamIncludes = new List<string> {
                "Users"
            };
        }

        public async Task<Team> CreateTeam(string teamName, string description, string ownerId) {
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");
            if (description == null) throw new ArgumentException("Missing parameter description");
            if (ownerId == null) throw new ArgumentException("Missing parameter ownerId");

            User owner = await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Email == ownerId
                }
            });
            if (owner == null) throw new ArgumentException($"Unable to find user with id {ownerId}");

            return await _teamDB.Create(new() {
                Name = teamName,
                Description = description,
                TeamOwner = ownerId,
                Users = new HashSet<User> {
                    owner
                }
            });
        }

        public async Task<TeamJoinRequest> CreateRequest(string teamName, string userId) {
            if (userId == null) throw new ArgumentException("Missing parameter userId");
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");
            //Check if user has already requested to join team
            TeamJoinRequest request = await _joinRequestDB.FindSingle(new() {
                Conditions = new List<Func<TeamJoinRequest, bool>> {
                    r => r.UserId == userId,
                    r => r.TeamName == teamName
                }
            });

            if (request != null) throw new ArgumentException($"The user \"{userId}\" already has a pending request to join this team");

            //Create request
            return await _joinRequestDB.Create(new() {
                TeamName = teamName,
                UserId = userId
            });
        }

        public async Task<IList<TeamJoinRequest>> GetRequestsByTeamName(string teamName) {
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");

            return await _joinRequestDB.Query(new() {
                Conditions = new List<Func<TeamJoinRequest, bool>> {
                    r => r.TeamName == teamName
                },
                Includes = new List<string> {
                    "User"
                }
            });
        }

        public async Task<bool> ApproveOrDenyRequest(string requestId, bool approve = true) {
            if (requestId == null) throw new ArgumentException("Missing parameter requestId");
            //Check if user has already requested to join team
            TeamJoinRequest request = await _joinRequestDB.FindSingle(new() {
                Conditions = new List<Func<TeamJoinRequest, bool>> {
                    r => r.Id == requestId
                }
            });

            if (request == null) throw new ArgumentException($"No request with id \"{requestId}\" could be found");

            // Add User to team if approved
            if (approve) {
                //Get team
                Team targetTeam = await _teamDB.FindSingle(new() {
                    Conditions = new List<Func<Team, bool>> {
                        t => t.Name == request.TeamName
                    },
                    Includes = _teamIncludes
                });
                if (targetTeam == null) throw new ArgumentException($"Unable to load team with name \"{request.TeamName}\"specified in request");

                //Get user
                User targetUser = await _userDB.FindSingle(new() { 
                    Conditions = new List<Func<User, bool>> {
                        u => u.Email == request.UserId
                    }
                });
                if (targetUser == null) throw new ArgumentException("Unable to load user specified in request");

                //Add user to team
                targetTeam.Users.Add(targetUser);
            }

            //Remove request
            return await _joinRequestDB.Delete(request);
        }

        public async Task<bool> LeaveTeam(string userId, string teamName) {
            if (userId == null) throw new ArgumentException("Missing parameter userId");
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");
            //Get team
            Team targetTeam = await _teamDB.FindSingle(new() {
                Conditions = new List<Func<Team, bool>> {
                        t => t.Name == teamName
                    },
                Includes = _teamIncludes
            });
            if (targetTeam == null) throw new ArgumentException($"Unable to load team with name \"{teamName}\"");

            //Remove target user from team
            User targetUser = targetTeam.Users.SingleOrDefault(u => u.Email == userId);
            if (targetUser == null) return false;

            targetTeam.Users.Remove(targetUser);

            return true;
        }

        public async Task<bool> DisbandTeam(string userId, string teamName) {
            if (userId == null) throw new ArgumentException("Missing parameter userId");
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");

            //Get team
            Team targetTeam = await _teamDB.FindSingle(new() {
                Conditions = new List<Func<Team, bool>> {
                        t => t.Name == teamName
                    },
                Includes = _teamIncludes
            });
            if (targetTeam == null) throw new ArgumentException($"Unable to load team with name \"{teamName}\"");

            //Delete team if the user is the owner
            if (targetTeam.TeamOwner != null && targetTeam.TeamOwner != userId) throw new ArgumentException($"User \"{userId}\" is not the owner of this team");
            return await _teamDB.Delete(targetTeam);
        }
    }
}