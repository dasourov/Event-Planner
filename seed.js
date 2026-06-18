// =============================================
// EventPlanner - Full MongoDB Seed Script
// Run with: docker exec eventplanner_mongodb mongosh eventplanner /seed.js
// =============================================

var db = db.getSiblingDB('eventplanner');

// ---- CATEGORIES ----
var existingCats = db.Categories.countDocuments();
if (existingCats === 0) {
  db.Categories.insertMany([
    { name: 'Technology',  description: 'Tech conferences, workshops, and meetups' },
    { name: 'Sports',      description: 'Athletic events, games, and outdoor activities' },
    { name: 'Music',       description: 'Concerts, festivals, and musical performances' },
    { name: 'Art',         description: 'Exhibitions, galleries, and creative workshops' },
    { name: 'Networking',  description: 'Professional networking and career events' },
    { name: 'Wellness',    description: 'Yoga, meditation, and mindfulness gatherings' },
    { name: 'Workshop',    description: 'Hands-on learning and skill-building sessions' },
    { name: 'Food',        description: 'Culinary experiences, tastings, and cooking classes' }
  ]);
  print('✓ Seeded 8 categories');
} else {
  print('- Categories already seeded (' + existingCats + ')');
}

var cats = {};
db.Categories.find().forEach(function(c) { cats[c.name] = c._id.toString(); });

// ---- USERS (only admin — use the app's C# seeder for a valid password hash) ----
var existingUsers = db.Users.countDocuments();
if (existingUsers === 0) {
  var now = new Date();
  db.Users.insertOne({
    username: 'admin',
    email: 'admin@eventplanner.com',
    passwordHash: '$2a$11$PLACEHOLDER_admin',
    role: 1,
    isBanned: false,
    createdAt: now
  });
  print('✓ Seeded admin user');
} else {
  print('- Users already seeded (' + existingUsers + ')');
}

var users = {};
db.Users.find().forEach(function(u) { users[u.username] = u._id.toString(); });

// ---- EVENTS ----
var existingEvents = db.Events.countDocuments();
if (existingEvents === 0) {
  var now = new Date();
  var addDays = function(d, n) { return new Date(d.getTime() + n*86400000); };
  db.Events.insertMany([
    { title: '.NET Aspire Developer Summit',    description: 'Learn how to build cloud-native apps with .NET Aspire, MongoDB, and modern microservices patterns.', location: 'Munich Convention Center, Germany', latitude: 48.1351, longitude: 11.5820, date: addDays(now,30),  organizerId: users['alice'],   status: 1, categoryId: cats['Technology'], maxAttendees: 200,   createdAt: now },
    { title: 'Munich City Marathon 2026',       description: "Annual city marathon through Munich's most scenic routes. All skill levels welcome.",                location: 'Olympiapark, Munich',             latitude: 48.1755, longitude: 11.5518, date: addDays(now,45),  organizerId: users['bob'],     status: 1, categoryId: cats['Sports'],     maxAttendees: 5000,  createdAt: now },
    { title: 'Live Jazz Night',                 description: 'An intimate evening with award-winning jazz performers in a candlelit venue.',                       location: 'Blue Note Club, Berlin',          latitude: 52.5200, longitude: 13.4050, date: addDays(now,10),  organizerId: users['charlie'], status: 1, categoryId: cats['Music'],      maxAttendees: 80,    createdAt: now },
    { title: 'Modern Art Exhibition Opening',   description: 'Showcasing emerging European contemporary artists. Free admission, refreshments served.',            location: 'Pinakothek der Moderne, Munich',  latitude: 48.1497, longitude: 11.5723, date: addDays(now,7),   organizerId: users['alice'],   status: 1, categoryId: cats['Art'],        maxAttendees: null,  createdAt: now },
    { title: 'Startup Pitch Night',             description: 'Founders pitch to investors. Network with VCs, angel investors, and fellow entrepreneurs.',          location: 'WeWork, Munich',                  latitude: 48.1372, longitude: 11.5755, date: addDays(now,21),  organizerId: users['admin'],   status: 1, categoryId: cats['Networking'], maxAttendees: 100,   createdAt: now },
    { title: 'Sunrise Yoga in the Park',        description: 'Start your day with vinyasa flow yoga as the sun rises. Mats provided.',                            location: 'Englischer Garten, Munich',       latitude: 48.1530, longitude: 11.5917, date: addDays(now,5),   organizerId: users['diana'],   status: 1, categoryId: cats['Wellness'],   maxAttendees: 30,    createdAt: now },
    { title: 'React 19 Workshop',               description: "Deep dive into React 19's new features: actions, server components, and the use() hook.",           location: 'Online (Zoom)',                   latitude: null,    longitude: null,    date: addDays(now,14),  organizerId: users['bob'],     status: 1, categoryId: cats['Workshop'],   maxAttendees: 50,    createdAt: now },
    { title: 'Internal Team Hackathon (Draft)', description: 'Private hackathon for the engineering team. Not yet published.',                                     location: 'TBD',                             latitude: null,    longitude: null,    date: addDays(now,60),  organizerId: users['alice'],   status: 0, categoryId: cats['Technology'], maxAttendees: 40,    createdAt: now },
    { title: 'Spring Music Festival',           description: 'Three-day outdoor music festival featuring 30+ artists. Cancelled due to venue issues.',             location: 'Theresienwiese, Munich',          latitude: 48.1316, longitude: 11.5495, date: addDays(now,40),  organizerId: users['charlie'], status: 2, categoryId: cats['Music'],      maxAttendees: 10000, createdAt: now },
    { title: 'Tech Career Fair',                description: 'Connect with top tech companies hiring engineers, designers, and product managers.',                 location: 'TU Munich, Garching Campus',      latitude: 48.2628, longitude: 11.6692, date: addDays(now,28),  organizerId: users['admin'],   status: 1, categoryId: cats['Networking'], maxAttendees: 500,   createdAt: now }
  ]);
  print('✓ Seeded 10 events');
} else {
  print('- Events already seeded (' + existingEvents + ')');
}

var events = [];
db.Events.find({ status: 1 }).forEach(function(e) { events.push(e); });

// ---- BOOKINGS ----
var existingBookings = db.Bookings.countDocuments();
if (existingBookings === 0 && events.length > 0) {
  var now = new Date();
  var bookings = [];
  events.forEach(function(ev) {
    bookings.push({ userId: users['user'], eventId: ev._id.toString(), bookedAt: now });
  });
  if (events.length >= 2) bookings.push({ userId: users['alice'],   eventId: events[1]._id.toString(), bookedAt: now });
  if (events.length >= 3) { bookings.push({ userId: users['bob'],     eventId: events[2]._id.toString(), bookedAt: now }); bookings.push({ userId: users['charlie'], eventId: events[2]._id.toString(), bookedAt: now }); }
  if (events.length >= 4) bookings.push({ userId: users['charlie'], eventId: events[3]._id.toString(), bookedAt: now });
  db.Bookings.insertMany(bookings);
  print('✓ Seeded ' + bookings.length + ' bookings');
} else {
  print('- Bookings already seeded (' + existingBookings + ')');
}

// ---- COMMENTS ----
var existingComments = db.Comments.countDocuments();
if (existingComments === 0 && events.length > 0) {
  var now = new Date();
  var comments = [];
  if (events.length >= 1) {
    var ev = events[0];
    comments.push({ eventId: ev._id.toString(), userId: users['user'],    content: 'Looking forward to this!',                                    createdAt: new Date(now - 3*3600000) });
    comments.push({ eventId: ev._id.toString(), userId: users['charlie'], content: 'Will the sessions be recorded?',                             createdAt: new Date(now - 2*3600000) });
    comments.push({ eventId: ev._id.toString(), userId: users['alice'],   content: 'Yes, recordings will be available on-demand for attendees.', createdAt: new Date(now - 1*3600000) });
  }
  if (events.length >= 2) {
    var ev = events[1];
    comments.push({ eventId: ev._id.toString(), userId: users['user'], content: 'Where is the start line exactly?',             createdAt: new Date(now - 5*3600000) });
    comments.push({ eventId: ev._id.toString(), userId: users['bob'],  content: 'Start is at the main entrance of Olympiapark.', createdAt: new Date(now - 4*3600000) });
  }
  if (events.length >= 3) {
    comments.push({ eventId: events[2]._id.toString(), userId: users['charlie'], content: "Can't wait for tonight!", createdAt: new Date(now - 30*60000) });
  }
  if (events.length >= 4) {
    var ev = events[3];
    comments.push({ eventId: ev._id.toString(), userId: users['alice'], content: 'Excited to showcase the new works tonight.', createdAt: new Date(now - 8*3600000) });
    comments.push({ eventId: ev._id.toString(), userId: users['user'],  content: 'Will there be guided tours?',                 createdAt: new Date(now - 2*3600000) });
  }
  db.Comments.insertMany(comments);
  print('✓ Seeded ' + comments.length + ' comments');
} else {
  print('- Comments already seeded (' + existingComments + ')');
}

print('');
print('=== Final Counts ===');
print('Categories : ' + db.Categories.countDocuments());
print('Users      : ' + db.Users.countDocuments());
print('Events     : ' + db.Events.countDocuments());
print('Bookings   : ' + db.Bookings.countDocuments());
print('Comments   : ' + db.Comments.countDocuments());
