# RSS News Reader

Create your own account, subscribe on rss feeds and receive updates.

## API

- PUT /api/session/sign-up BODY=username+email+password (signup)
- POST /api/session/sign-in BODY=username+password RESPONSE=token (signin)
- PUT /api/feeds BODY=feed_link (subscribe on feed), checks whether the channel already exists in the database, adds it if not, and creates a subscription
- GET /api/feeds (return followings)
- GET /api/posts/isunread/{sinceDate} (return posts) return unread posts since the inputted date (in format dd-mm-yyyy)
- PUT /api/posts/isread BODY=postId (mark posts as read)
