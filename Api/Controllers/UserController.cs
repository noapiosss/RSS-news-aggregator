using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Contracts.Database;
using Contracts.Http;
using domain.Queries;
using Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            CreateUserCommand command = new()
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            };

            CreateUserCommandResult result = await _mediator.Send(command, cancellationToken);

            if (!result.IsRegistrationSuccessful)
            {
                return BadRequest(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest); // handle
            }

            CreateUserResponse response = new()
            {
                IsRegistrationSuccessful = result.IsRegistrationSuccessful,
                UsernameIsAlreadyInUse = result.UsernameIsAlreadyInUse,
                EmailIsAlreadyInUse = result.EmailIsAlreadyInUse
            };

            return Ok(response);
        }

        [HttpPut("{username}/feeds")]
        public async Task<IActionResult> AddFeed([FromRoute] string username, [FromBody] AddFeedRequest request, CancellationToken cancellationToken)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null ||
                HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value != username)
            {
                return BadRequest(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
            }

            using (XmlReader reader = XmlReader.Create(request.Url))
            {
                SyndicationFeed rssFeed = SyndicationFeed.Load(reader);
                Feed feed = new()
                {
                    Title = rssFeed.Title.Text,
                    Image = rssFeed.ImageUrl == null ? "" : rssFeed.ImageUrl.ToString(),
                    Description = rssFeed.Description.Text,
                    Link = rssFeed.Links.FirstOrDefault().Uri.ToString(),
                    LastUpdate = new()
                };

                CreateFeedCommand createFeedCommand = new()
                {
                    Feed = feed
                };
                CreateFeedCommandResult createFeedCommandResult = await _mediator.Send(createFeedCommand, cancellationToken);

                List<Post> newPosts = new();
                foreach (SyndicationItem post in rssFeed.Items.Where(i => i.PublishDate.UtcDateTime > createFeedCommandResult.Feed.LastUpdate))
                {
                    newPosts.Add(new()
                    {
                        Title = post.Title.Text,
                        Description = post.Summary.Text,
                        Link = post.Links.FirstOrDefault().Uri.ToString(),
                        PubDate = post.PublishDate.UtcDateTime
                    });
                }

                UpdateFeedPostsCommand updateFeedPostsCommand = new()
                {
                    Feed = createFeedCommandResult.Feed,
                    Posts = newPosts.OrderBy(np => np.PubDate).ToList()
                };
                UpdateFeedPostsCommandResult updateFeedPostsCommandResult = await _mediator.Send(updateFeedPostsCommand, cancellationToken);

                SubscribeToFeedCommand subscribeToFeedCommand = new()
                {
                    Username = username,
                    Feed = createFeedCommandResult.Feed
                };
                SubscribeToFeedCommandResult result = await _mediator.Send(subscribeToFeedCommand, cancellationToken);
            }

            return Ok();
        }

        [HttpGet("{username}/feeds")]
        public async Task<IActionResult> GetFeeds([FromRoute] string username, CancellationToken cancellationToken)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null ||
                HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value != username)
            {
                return BadRequest(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
            }

            GetFeedsQuery getFeedsQuery = new()
            {
                Username = username
            };
            GetFeedsQueryResult getFeedsQueryResult = await _mediator.Send(getFeedsQuery, cancellationToken);

            return Ok(getFeedsQueryResult.Feeds);
        }

        [HttpGet("{username}/posts/isunread/{sinceDateRequest}")] // dd-mm-yyyy
        public async Task<IActionResult> GetUnreadPostsSinceDate([FromRoute] string username, string sinceDateRequest, CancellationToken cancellationToken)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null ||
                HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value != username)
            {
                return BadRequest(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
            }

            string[] date = sinceDateRequest.Split("-");
            int year = Int32.Parse(date[2]);
            int month = Int32.Parse(date[1]);
            int day = Int32.Parse(date[0]);

            DateTime sinceDate = new(year, month, day);

            GetUnreadPostsSinceDateQuery getUnreadPostsSinceDateQuery = new()
            {
                Username = username,
                SinceDate = sinceDate
            };
            GetUnreadPostsSinceDateQueryResult getUnreadPostsSinceDateQueryResult = await _mediator.Send(getUnreadPostsSinceDateQuery, cancellationToken);

            return Ok(getUnreadPostsSinceDateQueryResult.Posts);
        }
        
        [HttpPut("{username}/posts/isread")]
        public async Task<IActionResult> MarkAsReadById([FromRoute] string username, [FromBody] int postId, CancellationToken cancellationToken)
        {
            if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) == null ||
                HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value != username)
            {
                return BadRequest(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
            }

            MarkAsReadByIdCommand markAsReadByIdCommand = new()
            {
                Username = username,
                PostId = postId
            };
            MarkAsReadByIdCommandResult markAsReadByIdCommandResult = await _mediator.Send(markAsReadByIdCommand, cancellationToken);

            return Ok(markAsReadByIdCommandResult.IsRead);
        }
    }
}