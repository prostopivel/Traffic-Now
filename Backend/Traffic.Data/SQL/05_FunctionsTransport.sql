create or replace function select_transport(CurrentTransportId uuid)
returns table (
	Id uuid,
	PointId uuid,
	Url varchar(100)
) as $$
begin
	return query
	select t.Id, t.PointId, t.Url from Transport t
	where t.Id = CurrentTransportId;
end;
$$ language plpgsql;

create or replace function select_transport_users(CurrentTransportId uuid)
returns table (
	UserId uuid
) as $$
begin
	return query
	select ut.UserId from Users_Transport ut
	where ut.TransportId = CurrentTransportId;
end;
$$ language plpgsql;

create or replace function select_transport_url()
returns table (
	TransportId uuid,
	TransportUrl varchar(100)
) as $$
begin
	return query
	select distinct t.Id, t.Url from Transport t;
end;
$$ language plpgsql;

create or replace function add_transport_user(CurrentTransportId uuid, TransportUserId uuid)
returns uuid as $$
begin
	insert into Users_Transport (UserId, TransportId)
	values (TransportUserId, CurrentTransportId)
	on conflict (UserId, TransportId) do nothing;

	return CurrentTransportId;
end;
$$ language plpgsql;

create or replace function delete_transport_user(CurrentTransportId uuid, TransportUserId uuid)
returns uuid as $$
begin
	delete from Users_Transport ut
	where ut.UserId = TransportUserId and ut.TransportId = CurrentTransportId;

	if CurrentTransportId not in (select ut_del.TransportId from Users_Transport ut_del) then
		perform delete_transport(CurrentTransportId);
	end if;

	return CurrentTransportId;
end;
$$ language plpgsql;

create or replace function create_transport(
	CurrentTransportId uuid,
	TransportPointId uuid,
	TransportUrl varchar(100),
	TransportUserId uuid)
returns uuid as $$
begin
	insert into Transport (Id, PointId, Url)
	values (CurrentTransportId, TransportPointId, TransportUrl);

	insert into Users_Transport (UserId, TransportId)
	values (TransportUserId, CurrentTransportId)
	on conflict (UserId, TransportId) do nothing;

	return CurrentTransportId;
end;
$$ language plpgsql;

create or replace function update_transport(
	TransportId uuid,
	TransportPointId uuid,
	TransportUrl varchar(100),
	UserId uuid)
returns uuid as $$
begin
	update Transport
	set PointId = TransportPointId
	where Id = TransportId;
	
	return TransportId;
end;
$$ language plpgsql;

create or replace function delete_transport(TransportId uuid)
returns uuid as $$
begin
	delete from Transport
	where Id = TransportId;
	
	return TransportId;
end;
$$ language plpgsql;

create or replace function select_user_transport(CurrentUserId uuid)
returns table (
	Id uuid,
	PointId uuid,
	Url varchar(100)
) as $$
begin
	return query
	select t.Id, t.PointId, t.Url from Transport t
	join Users_Transport ut on ut.TransportId = t.Id
	where ut.UserId = CurrentUserId;
end;
$$ language plpgsql;

create or replace function select_map_user_transport(CurrentMapId uuid, CurrentUserId uuid)
returns table (
	Id uuid,
	PointId uuid,
	Url varchar(100)
) as $$
begin
	return query
	select t.Id, t.PointId, t.Url from select_user_transport(CurrentUserId) t
	join Points p on t.PointId = p.Id
	join Maps m on p.MapId = m.Id
	where m.Id = CurrentMapId;
end;
$$ language plpgsql;

select * from Users_Transport;

select * from Transport;

select * from Users where Id = '75bd1f37-c647-4c27-9ce2-3999b8f09732';