---
title: Admin UI
---

# Admin UI

The EventStoreDB Admin UI is available at _SERVER_IP:2113_ and helps you visually interact with and manage a cluster. This guide explains the tabs of the interface and what they do.

::: tip
The embedded EventStoreDB Admin UI web interface is reaching its end of life. You can try using the Early Preview of [Event Store Navigator](https://learn.eventstore.com/event-store-navigator-preview) app instead. It doesn't have full feature parity with the embedded web UI, but you will find some usability improvements already in place.
:::

## Dashboard

![Web admin interface dashboard](./images/wai-dashboard.png)

The top portion of the _Dashboard_ tab provides an overview of active queues with associated statistics. The _+_ icon indicates a queue group. Clicking any  _+_ icon reveals the queues associated with the group.

The bottom portion of the tab provides information about the active connections to EventStoreDB.

Clicking the _Snapshot_ button in the upper right will produce a snapshot of all current queue statistics.

## Stream browser

![Web admin interface stream browser tab](./images/wai-stream-browser.png)

The _Stream Browser_ tab provides an overview of recently created and changed streams. Click on any individual stream name to access the stream details.

### Event stream

![Web admin interface stream details](./images/wai-stream-details.png)

Each stream shows pages of the events in a stream with an overview of the event. Click the _Name_ to see the EventId, and _JSON_ to see the event data. The buttons above change depending on what you are viewing in the interface. The _Back_ button takes you to the parent screen.

The buttons on the top right when you are viewing an event stream are:

- _Pause_: Stop showing events arriving into this stream.
- _Resume_: Resume showing events arriving into this stream.
- _Edit ACL_: Edit [the access control lists](security.md#access-control-lists) for a stream.
- _Add Event_: [Add a new event](@clients/http-api/README.md#appending-events) to the stream.
- _Delete_: [Delete a stream](@clients/http-api/README.md#deleting-a-stream) to the stream.
- _Query_: Open code editor field for quick analysis of your event streams.

The buttons on the left above the events when you are viewing an event stream are:

- _self_: Takes you to the overview of the stream.
- _first_: Takes you to the first page of events in a stream.
- _previous_: Takes you to the previous page of events in a stream.
- _metadata_: Shows the metadata of a stream.
  - On the metadata screen, click _Add New Like This_ to add a new event to the stream.

## Projections

![Web admin interface projections tab](./images/wai-projections.png)

The _Projections_ tab shows system and user created projections defined in EventStoreDB, the buttons above the list do the following:

- _Disable All_: Disable all running projections.
- _Enable All_: Enable all stopped projections.
- _Include Queries_: Toggle displaying queries in the Projections table.
- _New Projection_: [Create a user-defined projection](projections.md#user-defined-projections) with the Admin UI.

Clicking an individual projection shows further details.

![Web admin interface projection details](./images/wai-projection-details.jpg)

On the left is the projection definition. On the right are the stats, results, and state of the projection. The buttons above the details do the following:

- _Start_: Start a stopped projection.
- _Stop_: Stop a running projection.
- _Edit_: Edit the projection definition.
- _Config_: [Set configuration options](projections.md#configuring-projections) for a projection.
- _Debug_: Opens [the debugging interface](projections.md#debugging) to debug what effect a projection is having on events.
- _Delete_: Delete a projection.
- _Reset_: Reset a projection.
- _Back_: Returns you to the parent screen.

## Query

The _Query_ tab has a code editor field where you can create transient and short-lived projections for quick analysis of your event streams.

![Web admin interface query details](./images/wai-query-details.png)

## Persistent subscriptions

The _Persistent Subscriptions_ tab shows an overview of [persistent subscriptions](persistent-subscriptions.md) configured on streams. The button above the list does the following:

- _New Subscription_: Create a new subscription.

Clicking the _+_ icon next to a stream name reveals the subscription name and more buttons. The _Back_ button takes you to the parent screen.

- _Edit_: Edit the subscription.
- _Delete_: Delete the subscription.
- _Detail_: Shows the subscription configuration options.
- _Replay Parked Messages_: Replay events in subscription to return state.

## Admin

![Web admin interface projections tab](./images/wai-admin.png)

The _Admin_ tab shows subsystems enabled (currently only [projections](projections.md)) on EventStoreDB and [scavenges](operations.md#scavenging) run. You can start a new scavenge operation by clicking the _Scavenge_ button, and shut EventStoreDB down by clicking the _Shutdown Server_ button.

## Users

![Web admin interface projections tab](./images/wai-users.png)

The _Users_ tab shows [the users defined in EventStoreDB](security.md#authentication), clicking an individual user shows a JSON representation of that user's details.

## Log out

Logs you out of the Admin UI interface.
