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

        public async Task<IList<Team>> GetAllTeams() {
            return await _teamDB.Query(new() {
                Includes = new List<string> {
                    "Users"
                }
            });
        }

        public async Task<Team> GetTeamByName(string teamName) {
            return await _teamDB.FindSingle(new() {
                Includes = new List<string> {
                    "Users"
                },
                Conditions = new List<Func<Team, bool>> {
                    t => t.Name == teamName
                }
            });
        }


        public async Task<Team> CreateTeam(string teamName, string description, string ownerId) {
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");
            if (description == null) throw new ArgumentException("Missing parameter description");
            if (ownerId == null) throw new ArgumentException("Missing parameter ownerId");

            User owner = await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Id == ownerId
                }
            });
            if (owner == null) throw new ArgumentException($"Unable to find user with ID '{ownerId}'");
            if (owner.TeamId != null) throw new ArgumentException($"User is already a member of team with ID'{owner.TeamId}'");

            return await _teamDB.Create(new() {
                Name = teamName,
                Description = description,
                TeamOwner = ownerId,
                Users = new HashSet<User> {
                    owner
                }
            });
        }

        public async Task<TeamJoinRequest> CreateRequest(string userId, string teamName) {
            if (userId == null) throw new ArgumentException("Missing parameter userId");
            if (teamName == null) throw new ArgumentException("Missing parameter teamName");

            //Check if team exists
            Team targetTeam = await _teamDB.FindSingle(new() {
                Conditions = new List<Func<Team, bool>> {
                    t => t.Name == teamName
                }
            });
            if (targetTeam == null) throw new ArgumentException($"Unable to load team with name '{teamName}'");

            //Check if user exists
            User targetUser= await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Id == userId
                }
            });
            if (targetUser == null) throw new ArgumentException($"Unable to load user with ID '{userId}'");

            //Check if user has already requested to join team
            TeamJoinRequest request = await _joinRequestDB.FindSingle(new() {
            Conditions = new List<Func<TeamJoinRequest, bool>> {
                r => r.UserId == userId,
                r => r.TeamName == teamName
            }
            });

            if (request != null) throw new ArgumentException($"The user with ID '{userId}' already has a pending request to join this team");

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

        public async Task<bool> ApproveOrDenyRequest(string requestId, string ownerId, bool approve = true) {
            if (requestId == null) throw new ArgumentException("Missing parameter requestId");
            
            //Search for request
            TeamJoinRequest request = await _joinRequestDB.FindSingle(new() {
                Conditions = new List<Func<TeamJoinRequest, bool>> {
                    r => r.Id == requestId
                }
            });

            if (request == null) throw new ArgumentException($"No request with id '{requestId}' could be found");

            // Add User to team if approved
            if (approve) {
                //Get team
                Team targetTeam = await _teamDB.FindSingle(new() {
                    Conditions = new List<Func<Team, bool>> {
                        t => t.Name == request.TeamName
                    },
                    Includes = _teamIncludes
                });
                if (targetTeam == null) throw new ArgumentException($"Unable to load team with name '{request.TeamName}'specified in request");
                if (targetTeam.TeamOwner != ownerId) throw new ArgumentException($"User with ID '{ownerId}' cannot approve requests. Only the owner with ID '{targetTeam.TeamOwner}' can approve requests.");

                //Get user
                User targetUser = await _userDB.FindSingle(new() { 
                    Conditions = new List<Func<User, bool>> {
                        u => u.Id == request.UserId
                    }
                });
                if (targetUser == null) throw new ArgumentException("Unable to load user specified in request");

                //Add user to team
                targetTeam.Users.Add(targetUser);
            }

            //Remove request
            return await _joinRequestDB.Delete(request);
        }

        public async Task<bool> LeaveTeam(string userId) {
            if (userId == null) throw new ArgumentException("Missing parameter userId");

            User targetUser = await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Id == userId
                }
            });

            if (targetUser == null) throw new ArgumentException($"Unable to load user with ID '{userId}'");
            if (targetUser.TeamId == null) throw new ArgumentException($"User with ID '{userId}' is not a member of a team");

            //Get team
            Team targetTeam = await _teamDB.FindSingle(new() {
                Conditions = new List<Func<Team, bool>> {
                        t => t.Id == targetUser.TeamId
                    },
                Includes = _teamIncludes
            });
            if (targetTeam == null) throw new ArgumentException($"Unable to load team with ID '{targetUser.Id}'");

            //Remove target user from team
            targetTeam.Users.Remove(targetUser);
            targetUser.Team = null;
            targetUser.TeamId = null;
            return await _teamDB.Save();
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
            if (targetTeam == null) throw new ArgumentException($"Unable to load team with name '{teamName}'");

            //Delete team if the user is the owner
            if (targetTeam.TeamOwner != null && targetTeam.TeamOwner != userId) throw new ArgumentException($"User '{userId}' is not the owner of this team");
            return await _teamDB.Delete(targetTeam);
        }
    }
}