using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Api.Helpers.Interfaces;
using Contracts.Database;
using Contracts.Http;
using Domain.Queries;
using Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Route("api")]
    public class UserController : BaseCotroller
    {
        private readonly IMediator _mediator;
        private readonly ISyndicationConverter _syndicationConverter;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserService _userService;
        public UserController(IMediator mediator,
            ISyndicationConverter syndicationConverter,
            ITokenHandler tokenHandler,
            IUserService userService,
            ILogger<UserController> logger) : base(logger)
        {
            _mediator = mediator;
            _syndicationConverter = syndicationConverter;
            _tokenHandler = tokenHandler;
            _userService = userService;
        }

        /// <summary>
        /// Subscribe to feed
        /// </summary>
        /// <returns>Feed</returns>
        /// <response code="200">Return feed in json fromat</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("feeds"), Authorize]
        public Task<IActionResult> AddFeed([FromBody] AddFeedRequest request, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_userService.IsAuthorized(out string username))
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.Unauthorized,
                        Message = "Unauthorized"
                    });
                }

                try
                {
                    using XmlReader reader = XmlReader.Create(request.Url);
                    SyndicationFeed syndicationFeed = SyndicationFeed.Load(reader);

                    Feed feed = _syndicationConverter.SyndicationFeedToFeed(syndicationFeed, request.Url);

                    CreateFeedCommand createFeedCommand = new()
                    {
                        Feed = feed
                    };
                    CreateFeedCommandResult createFeedCommandResult = await _mediator.Send(createFeedCommand, cancellationToken);

                    List<Post> posts = new();
                    foreach (SyndicationItem syndicationItem in syndicationFeed.Items.Where(i => i.PublishDate.UtcDateTime > createFeedCommandResult.Feed.LastUpdate))
                    {
                        posts.Add(_syndicationConverter.SyndicationItemToPost(syndicationItem, createFeedCommandResult.Feed.Id));
                    }

                    UpdateFeedPostsCommand updateFeedPostsCommand = new()
                    {
                        FeedId = createFeedCommandResult.Feed.Id,
                        Posts = posts.OrderBy(p => p.PubDate).ToList()
                    };
                    UpdateFeedPostsCommandResult updateFeedPostsCommandResult = await _mediator.Send(updateFeedPostsCommand, cancellationToken);

                    SubscribeToFeedCommand subscribeToFeedCommand = new()
                    {
                        Username = username,
                        Feed = createFeedCommandResult.Feed
                    };
                    SubscribeToFeedCommandResult result = await _mediator.Send(subscribeToFeedCommand, cancellationToken);

                    return Ok(createFeedCommandResult.Feed);
                }
                catch
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.BadRequest,
                        Message = "Invalid url"
                    });
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Return channels you've subscribed to
        /// </summary>
        /// <returns>List of feeds</returns>
        /// <response code="200">Return feeds in json fromat</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("feeds"), Authorize]
        public Task<IActionResult> GetFeeds(CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_userService.IsAuthorized(out string username))
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.Unauthorized,
                        Message = "Unauthorized"
                    });
                }

                GetUserFeedsQuery getUserFeedsQuery = new()
                {
                    Username = username
                };
                GetUserFeedsQueryResult getUserFeedsQueryResult = await _mediator.Send(getUserFeedsQuery, cancellationToken);

                return Ok(getUserFeedsQueryResult.Feeds);
            }, cancellationToken);
        }

        /// <summary>
        /// Return unread posts since the inputted date
        /// </summary>
        /// <returns>List of posts</returns>
        /// <param name="sinceDateRequest">Should be in format dd-mm-yyyy</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Return psots in json fromat</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("posts/isunread/{sinceDateRequest}"), Authorize]
        public Task<IActionResult> GetUnreadPostsSinceDate(string sinceDateRequest, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_userService.IsAuthorized(out string username))
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.Unauthorized,
                        Message = "Unauthorized"
                    });
                }

                if (!DateTime.TryParse(sinceDateRequest, out DateTime sinceDate))
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.BadRequest,
                        Message = "Invalid date format"
                    });
                }

                GetUnreadPostsSinceDateQuery getUnreadPostsSinceDateQuery = new()
                {
                    Username = username,
                    SinceDate = sinceDate
                };
                GetUnreadPostsSinceDateQueryResult getUnreadPostsSinceDateQueryResult = await _mediator.Send(getUnreadPostsSinceDateQuery, cancellationToken);

                return Ok(getUnreadPostsSinceDateQueryResult.Posts);
            }, cancellationToken);
        }

        /// <summary>
        /// Mark post as read by postId
        /// </summary>
        /// <returns>Request result</returns>
        /// <response code="200">Request status</response>
        /// <response code="403">Post not fount</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("posts/isread"), Authorize]
        public Task<IActionResult> MarkAsReadById([FromBody] int postId, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_userService.IsAuthorized(out string username))
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.Unauthorized,
                        Message = "Unauthorized"
                    });
                }

                MarkAsReadByIdCommand markAsReadByIdCommand = new()
                {
                    Username = username,
                    PostId = postId
                };
                MarkAsReadByIdCommandResult markAsReadByIdCommandResult = await _mediator.Send(markAsReadByIdCommand, cancellationToken);

                return Ok(markAsReadByIdCommandResult.IsRead);
            }, cancellationToken);
        }
    }
}