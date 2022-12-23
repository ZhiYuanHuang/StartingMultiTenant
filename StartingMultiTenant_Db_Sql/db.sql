--
-- PostgreSQL database dump
--

-- Dumped from database version 15.1 (Debian 15.1-1.pgdg110+1)
-- Dumped by pg_dump version 15.1

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: dbserver; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.dbserver (
    id bigint NOT NULL,
    dbtype integer DEFAULT 0 NOT NULL,
    serverhost character varying NOT NULL,
    serverport integer NOT NULL,
    username character varying,
    encryptuserpwd character varying,
    cancreatenew boolean DEFAULT true NOT NULL
);


ALTER TABLE public.dbserver OWNER TO postgres;

--
-- Name: DbServer_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."DbServer_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."DbServer_Id_seq" OWNER TO postgres;

--
-- Name: DbServer_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."DbServer_Id_seq" OWNED BY public.dbserver.id;


--
-- Name: createdbscript; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.createdbscript (
    id bigint NOT NULL,
    name character varying(100) NOT NULL,
    majorversion integer DEFAULT 0 NOT NULL,
    serviceidentifier character varying(50) NOT NULL,
    dbidentifier character varying(100) NOT NULL,
    dbnamewildcard character varying(50),
    binarycontent bytea,
    dbtype integer NOT NULL
);


ALTER TABLE public.createdbscript OWNER TO postgres;

--
-- Name: createdbscript_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.createdbscript_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.createdbscript_id_seq OWNER TO postgres;

--
-- Name: createdbscript_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.createdbscript_id_seq OWNED BY public.createdbscript.id;


--
-- Name: dbinfo; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.dbinfo (
    id bigint NOT NULL,
    name character varying(100),
    identifier character varying(50) NOT NULL,
    description character varying(200),
    serviceidentifier character varying(50) NOT NULL
);


ALTER TABLE public.dbinfo OWNER TO postgres;

--
-- Name: dbinfo_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.dbinfo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.dbinfo_id_seq OWNER TO postgres;

--
-- Name: dbinfo_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.dbinfo_id_seq OWNED BY public.dbinfo.id;


--
-- Name: externaltenantservicedbconn; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.externaltenantservicedbconn (
    id bigint NOT NULL,
    tenantidentifier character varying(50) NOT NULL,
    tenantdomain character varying(50) NOT NULL,
    serviceidentifier character varying(50) NOT NULL,
    dbidentifier character varying(50) NOT NULL,
    encryptedconnstr character varying(300),
    overrideencryptedconnstr character varying(300),
    updatetime timestamp without time zone
);


ALTER TABLE public.externaltenantservicedbconn OWNER TO postgres;

--
-- Name: externaltenantservicedbconn_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.externaltenantservicedbconn_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.externaltenantservicedbconn_id_seq OWNER TO postgres;

--
-- Name: externaltenantservicedbconn_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.externaltenantservicedbconn_id_seq OWNED BY public.externaltenantservicedbconn.id;


--
-- Name: historytenantservicedbconn; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.historytenantservicedbconn (
    id bigint NOT NULL,
    dbconnid bigint NOT NULL,
    createscriptname character varying(100),
    createscriptversion integer,
    curschemaversion integer,
    dbserverid bigint,
    encryptedconnstr character varying(300),
    actiontype integer NOT NULL
);


ALTER TABLE public.historytenantservicedbconn OWNER TO postgres;

--
-- Name: historytenantservicedbconn_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.historytenantservicedbconn_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.historytenantservicedbconn_id_seq OWNER TO postgres;

--
-- Name: historytenantservicedbconn_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.historytenantservicedbconn_id_seq OWNED BY public.historytenantservicedbconn.id;


--
-- Name: schemaupdatescript; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.schemaupdatescript (
    id bigint NOT NULL,
    name character varying(50) NOT NULL,
    binarycontent bytea,
    rollbackscriptbinarycontent bytea,
    createscriptname character varying(50) NOT NULL,
    basemajorversion integer NOT NULL,
    minorversion integer NOT NULL,
    dbnamewildcard character varying(50)
);


ALTER TABLE public.schemaupdatescript OWNER TO postgres;

--
-- Name: schemaupdatescript_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.schemaupdatescript_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.schemaupdatescript_id_seq OWNER TO postgres;

--
-- Name: schemaupdatescript_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.schemaupdatescript_id_seq OWNED BY public.schemaupdatescript.id;


--
-- Name: serviceinfo; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.serviceinfo (
    id bigint NOT NULL,
    name character varying(50),
    identifier character varying(50) NOT NULL,
    description character varying(100)
);


ALTER TABLE public.serviceinfo OWNER TO postgres;

--
-- Name: serviceinfo_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.serviceinfo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.serviceinfo_id_seq OWNER TO postgres;

--
-- Name: serviceinfo_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.serviceinfo_id_seq OWNED BY public.serviceinfo.id;


--
-- Name: tenantdomain; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tenantdomain (
    id bigint NOT NULL,
    tenantdomain character varying(50)
);


ALTER TABLE public.tenantdomain OWNER TO postgres;

--
-- Name: tenantdomain_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tenantdomain_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tenantdomain_id_seq OWNER TO postgres;

--
-- Name: tenantdomain_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tenantdomain_id_seq OWNED BY public.tenantdomain.id;


--
-- Name: tenantidentifier; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tenantidentifier (
    id bigint NOT NULL,
    tenantguid character varying(50) NOT NULL,
    tenantidentifier character varying(50),
    tenantdomain character varying(50)
);


ALTER TABLE public.tenantidentifier OWNER TO postgres;

--
-- Name: tenantidentifier_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tenantidentifier_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tenantidentifier_id_seq OWNER TO postgres;

--
-- Name: tenantidentifier_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tenantidentifier_id_seq OWNED BY public.tenantidentifier.id;


--
-- Name: tenantservicedbconn; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tenantservicedbconn (
    id bigint NOT NULL,
    tenantidentifier character varying(50) NOT NULL,
    tenantdomain character varying(50) NOT NULL,
    serviceidentifier character varying(100) NOT NULL,
    dbidentifier character varying(100) NOT NULL,
    createscriptname character varying(200) NOT NULL,
    createscriptversion integer DEFAULT 0 NOT NULL,
    curschemaversion integer DEFAULT 0,
    dbserverid bigint NOT NULL,
    encryptedconnstr character varying(300)
);


ALTER TABLE public.tenantservicedbconn OWNER TO postgres;

--
-- Name: tenantservicedbconn_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tenantservicedbconn_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tenantservicedbconn_id_seq OWNER TO postgres;

--
-- Name: tenantservicedbconn_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tenantservicedbconn_id_seq OWNED BY public.tenantservicedbconn.id;


--
-- Name: createdbscript id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.createdbscript ALTER COLUMN id SET DEFAULT nextval('public.createdbscript_id_seq'::regclass);


--
-- Name: dbinfo id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dbinfo ALTER COLUMN id SET DEFAULT nextval('public.dbinfo_id_seq'::regclass);


--
-- Name: dbserver id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dbserver ALTER COLUMN id SET DEFAULT nextval('public."DbServer_Id_seq"'::regclass);


--
-- Name: externaltenantservicedbconn id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.externaltenantservicedbconn ALTER COLUMN id SET DEFAULT nextval('public.externaltenantservicedbconn_id_seq'::regclass);


--
-- Name: historytenantservicedbconn id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.historytenantservicedbconn ALTER COLUMN id SET DEFAULT nextval('public.historytenantservicedbconn_id_seq'::regclass);


--
-- Name: schemaupdatescript id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schemaupdatescript ALTER COLUMN id SET DEFAULT nextval('public.schemaupdatescript_id_seq'::regclass);


--
-- Name: serviceinfo id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.serviceinfo ALTER COLUMN id SET DEFAULT nextval('public.serviceinfo_id_seq'::regclass);


--
-- Name: tenantdomain id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantdomain ALTER COLUMN id SET DEFAULT nextval('public.tenantdomain_id_seq'::regclass);


--
-- Name: tenantidentifier id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantidentifier ALTER COLUMN id SET DEFAULT nextval('public.tenantidentifier_id_seq'::regclass);


--
-- Name: tenantservicedbconn id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantservicedbconn ALTER COLUMN id SET DEFAULT nextval('public.tenantservicedbconn_id_seq'::regclass);


--
-- Name: dbserver DbServer_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dbserver
    ADD CONSTRAINT "DbServer_pkey" PRIMARY KEY (id);


--
-- Name: createdbscript createdbscript_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.createdbscript
    ADD CONSTRAINT createdbscript_pkey PRIMARY KEY (id);


--
-- Name: dbinfo dbinfo_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dbinfo
    ADD CONSTRAINT dbinfo_pkey PRIMARY KEY (id);


--
-- Name: externaltenantservicedbconn externaltenantservicedbconn_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.externaltenantservicedbconn
    ADD CONSTRAINT externaltenantservicedbconn_pkey PRIMARY KEY (id);


--
-- Name: historytenantservicedbconn historytenantservicedbconn_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.historytenantservicedbconn
    ADD CONSTRAINT historytenantservicedbconn_pkey PRIMARY KEY (id);


--
-- Name: schemaupdatescript schemaupdatescript_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schemaupdatescript
    ADD CONSTRAINT schemaupdatescript_pkey PRIMARY KEY (id);


--
-- Name: tenantdomain tenantdomain_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantdomain
    ADD CONSTRAINT tenantdomain_pkey PRIMARY KEY (id);


--
-- Name: tenantidentifier tenantidentifier_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantidentifier
    ADD CONSTRAINT tenantidentifier_pkey PRIMARY KEY (id);


--
-- Name: tenantservicedbconn tenantservicedbconn_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantservicedbconn
    ADD CONSTRAINT tenantservicedbconn_pkey PRIMARY KEY (id);


--
-- Name: createdbscript u_createdbscript_name_majorversion; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.createdbscript
    ADD CONSTRAINT u_createdbscript_name_majorversion UNIQUE (name, majorversion);


--
-- Name: dbinfo u_dbinfo_identifier; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dbinfo
    ADD CONSTRAINT u_dbinfo_identifier UNIQUE (serviceidentifier, identifier);


--
-- Name: externaltenantservicedbconn u_externaltenantservicedbconn_1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.externaltenantservicedbconn
    ADD CONSTRAINT u_externaltenantservicedbconn_1 UNIQUE (tenantidentifier, tenantdomain, serviceidentifier, dbidentifier);


--
-- Name: schemaupdatescript u_schemaupdatescript_1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schemaupdatescript
    ADD CONSTRAINT u_schemaupdatescript_1 UNIQUE (createscriptname, basemajorversion, minorversion);


--
-- Name: schemaupdatescript u_schemaupdatescript_name; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schemaupdatescript
    ADD CONSTRAINT u_schemaupdatescript_name UNIQUE (name);


--
-- Name: serviceinfo u_serviceinfo_identifier; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.serviceinfo
    ADD CONSTRAINT u_serviceinfo_identifier PRIMARY KEY (identifier);


--
-- Name: tenantdomain u_tenantdomain_tenantdomain; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantdomain
    ADD CONSTRAINT u_tenantdomain_tenantdomain UNIQUE (tenantdomain);


--
-- Name: tenantidentifier u_tenantidentifier_1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantidentifier
    ADD CONSTRAINT u_tenantidentifier_1 UNIQUE (tenantidentifier, tenantdomain);


--
-- Name: tenantidentifier u_tenantidentifier_tenantguid; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantidentifier
    ADD CONSTRAINT u_tenantidentifier_tenantguid UNIQUE (tenantguid);


--
-- Name: tenantservicedbconn u_tenantservicedbconn_1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tenantservicedbconn
    ADD CONSTRAINT u_tenantservicedbconn_1 UNIQUE (tenantidentifier, tenantdomain, createscriptname, createscriptversion);


--
-- Name: idx_historytenantservicedbconn_dbconnid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_historytenantservicedbconn_dbconnid ON public.historytenantservicedbconn USING btree (dbconnid);


--
-- PostgreSQL database dump complete
--

