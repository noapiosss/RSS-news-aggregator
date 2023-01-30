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

namespace Api.Controllers
{
    [Route("api/users")]
    public class UserController : BaseCotroller
    {
        private readonly IMediator _mediator;
        private readonly ISyndicationConverter _syndicationConverter;
        private readonly ITokenHandler _tokenHandler;
        public UserController(IMediator mediator,
            ISyndicationConverter syndicationConverter,
            ITokenHandler tokenHandler,
            ILogger<UserController> logger) : base(logger)
        {
            _mediator = mediator;
            _syndicationConverter = syndicationConverter;
            _tokenHandler = tokenHandler;
        }

        [HttpPut("{token}/feeds")]
        public Task<IActionResult> AddFeed([FromRoute] string token, [FromBody] AddFeedRequest request, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_tokenHandler.Validate(token, out string username))
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
                }
                catch
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.BadRequest,
                        Message = "Invalid url"
                    });
                }

                return Ok();
            }, cancellationToken);
        }

        [HttpGet("{token}/feeds")]
        public Task<IActionResult> GetFeeds([FromRoute] string token, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_tokenHandler.Validate(token, out string username))
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

        [HttpGet("{token}/posts/isunread/{sinceDateRequest}")] // dd-mm-yyyy
        public Task<IActionResult> GetUnreadPostsSinceDate([FromRoute] string token, string sinceDateRequest, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_tokenHandler.Validate(token, out string username))
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

        [HttpPut("{token}/posts/isread")]
        public Task<IActionResult> MarkAsReadById([FromRoute] string token, [FromBody] int postId, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!_tokenHandler.Validate(token, out string username))
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