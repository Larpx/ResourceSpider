#!/usr/bin/env python
# coding=utf-8

#import redis
import os
import logging
from struct import unpack
from socket import inet_ntoa
import argparse

from magnet_dht.crawler import start_server
from magnet_dht.magnet_to_torrent_aria2c import magnet2torrent
from magnet_dht.parse_torrent import parse_torrent

#crawler
import socket
import codecs
import time
from threading import Thread
from collections import deque
from multiprocessing import Process, cpu_count
import bencoder

#utils
# 每个节点长度
PER_NODE_LEN = 26
# 节点 id 长度
PER_NID_LEN = 20
# 节点 id 和 ip 长度
PER_NID_NIP_LEN = 24
# 构造邻居随机结点
NEIGHBOR_END = 14
# 日志等级
LOG_LEVEL = logging.INFO

#redis
# redis key
REDIS_KEY = "magnets"
# redis 地址
REDIS_HOST = "larpx-aliyun-redis.redis.zhangbei.rds.aliyuncs.com"
# redis 端口
REDIS_PORT = 6379
# redis 密码
REDIS_PASSWORD = "dhtspider:--237198606Hh"
# redis 连接池最大连接量
REDIS_MAX_CONNECTION = 20

#crawler
# 双端队列容量
MAX_NODE_QSIZE = 10000
# UDP 报文 buffsize
UDP_RECV_BUFFSIZE = 65535
# 服务 host
SERVER_HOST = "0.0.0.0"
# 服务端口
SERVER_PORT = 9090
# 磁力链接前缀
MAGNET_PER = "magnet:?xt=urn:btih:{}"
# while 循环休眠时间
SLEEP_TIME = 1e-5
# 节点 id 长度
PER_NID_LEN = 20
# 执行 bs 定时器间隔（秒）
PER_SEC_BS_TIMER = 8
# 是否使用全部进程
MAX_PROCESSES = cpu_count() // 2 or cpu_count()

#   main
def get_parser():
    """
    解析命令行参数
    """
    parser = argparse.ArgumentParser(description="start manage.py with flag.")
    parser.add_argument("-s", action="store_true", help="run start_server func.")
    parser.add_argument("-m", action="store_true", help="run magnet2torrent func")
    parser.add_argument("-p", action="store_true", help="run parse_torrent func")
    return parser

def command_line_runner():
    """
    执行命令行操作
    """
    parser = get_parser()
    args = vars(parser.parse_args())

    if args["s"]:
        start_server()
    elif args["m"]:
        magnet2torrent()
    elif args["p"]:
        parse_torrent()

if __name__ == "__main__":
    command_line_runner()

#   utils.py
def get_rand_id():
    """
    生成随机的节点 id，长度为 20 位
    """
    return os.urandom(PER_NID_LEN)

def get_neighbor(target):
    """
    生成随机 target 周边节点 id，在 Kademlia 网络中，距离是通过异或(XOR)计算的，
    结果为无符号整数。distance(A, B) = |A xor B|，值越小表示越近。

    :param target: 节点 id
    """
    return target[:NEIGHBOR_END] + get_rand_id()[NEIGHBOR_END:]

def get_nodes_info(nodes):
    """
    解析 find_node 回复中 nodes 节点的信息

    :param nodes: 节点薪资
    """
    length = len(nodes)
    # 每个节点单位长度为 26 为，node = node_id(20位) + node_ip(4位) + node_port(2位)
    if (length % PER_NODE_LEN) != 0:
        return []

    for i in range(0, length, PER_NODE_LEN):
        nid = nodes[i : i + PER_NID_LEN]
        # 利用 inet_ntoa 可以返回节点 ip
        ip = inet_ntoa(nodes[i + PER_NID_LEN : i + PER_NID_NIP_LEN])
        # 解包返回节点端口
        port = unpack("!H", nodes[i + PER_NID_NIP_LEN : i + PER_NODE_LEN])[0]
        yield (nid, ip, port)

def get_logger(logger_name):
    """
    返回日志实例
    """
    logger = logging.getLogger(logger_name)
    logger.setLevel(LOG_LEVEL)
    fh = logging.StreamHandler()
    fh.setFormatter(logging.Formatter("%(asctime)s - %(levelname)s - %(message)s"))
    logger.addHandler(fh)
    return logger

#   redis
class RedisClient:
    #def __init__(self, host=REDIS_HOST, port=REDIS_PORT, password=REDIS_PASSWORD):
    #    conn_pool = redis.ConnectionPool(
    #        host=host,
    #        port=port,
    #        password=password,
    #        max_connections=REDIS_MAX_CONNECTION,
    #    )
    #    self.redis = redis.Redis(connection_pool=conn_pool)

    #def add_magnet(self, magnet):
    #    """
    #    新增磁力链接
    #    """
    #    self.redis.sadd(REDIS_KEY, magnet)

    #def get_magnets(self, count=128):
    #    """
    #    返回指定数量的磁力链接
    #    """
    #    return self.redis.srandmember(REDIS_KEY, count)
    pass

#   mysql
class MySQLClient:
    #def __init__(self, host=REDIS_HOST, port=REDIS_PORT, password=REDIS_PASSWORD):
    #    conn_pool = redis.ConnectionPool(
    #        host=host,
    #        port=port,
    #        password=password,
    #        max_connections=REDIS_MAX_CONNECTION,
    #    )
    #    self.redis = redis.Redis(connection_pool=conn_pool)

    #def add_magnet(self, magnet):
    #    """
    #    新增磁力链接
    #    """
    #    self.redis.sadd(REDIS_KEY, magnet)

    #def get_magnets(self, count=128):
    #    """
    #    返回指定数量的磁力链接
    #    """
    #    return self.redis.srandmember(REDIS_KEY, count)
    pass


#       crawler


class HNode:
    def __init__(self, nid, ip=None, port=None):
        self.nid = nid
        self.ip = ip
        self.port = port

class DHTServer:
    def __init__(self, bind_ip, bind_port, process_id):
        self.bind_ip = bind_ip
        self.bind_port = bind_port
        self.process_id = process_id
        self.nid = get_rand_id()
        # nodes 节点是一个双端队列
        self.nodes = deque(maxlen=MAX_NODE_QSIZE)
        # KRPC 协议是由 bencode 编码组成的一个简单的 RPC 结构，使用 UDP 报文发送。
        self.udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, socket.IPPROTO_UDP)
        # UDP 地址绑定
        self.udp.bind((self.bind_ip, self.bind_port))
        # redis 客户端
        #self.rc = RedisClient()
        #self.logger = get_logger("logger_{}".format(bind_port))

    def bootstrap(self):
        """
        利用 tracker 服务器，伪装成 DHT 节点，加入 DHT 网络
        """
        for address in BOOTSTRAP_NODES:
            self.send_find_node(address)

    def bs_timer(self):
        """
        定时执行 bootstrap()
        """
        t = 1
        while True:
            if t % PER_SEC_BS_TIMER == 0:
                t = 1
                self.bootstrap()
            t += 1
            time.sleep(1)

    def send_krpc(self, msg, address):
        """
        发送 krpc 协议

        :param msg: 发送 UDP 报文信息
        :param address: 发送地址，(ip, port) 元组
        """
        try:
            # msg 要经过 bencode 编码
            self.udp.sendto(bencoder.bencode(msg), address)
        except:
            pass

    def send_error(self, tid, address):
        """
        发送错误回复
        """
        msg = dict(t=tid, y="e", e=[202, "Server Error"])
        self.send_krpc(msg, address)

    def send_find_node(self, address, nid=None):
        """
        发送 find_node 请求。

        :param address: 地址元组（ip, port)
        :param nid: 节点 id
        """
        nid = get_neighbor(nid) if nid else self.nid
        tid = get_rand_id()
        msg = dict(
            t=tid,
            y="q",
            q="find_node",
            a=dict(id=nid, target=get_rand_id()),
        )
        self.send_krpc(msg, address)

    def send_find_node_forever(self):
        """
        循环发送 find_node 请求
        """
        while True:
            try:
                node = self.nodes.popleft()
                self.send_find_node((node.ip, node.port), node.nid)
                time.sleep(SLEEP_TIME)
            except IndexError:
                self.bootstrap()

    def save_magnet(self, info_hash):
        """
        将磁力链接保存到数据库

        :param info_hash:  磁力链接的 info_hash
        """
        # 使用 codecs 解码 info_hash
        hex_info_hash = codecs.getencoder("hex")(info_hash)[0].decode()
        magnet = MAGNET_PER.format(hex_info_hash)
        self.rc.add_magnet(magnet)
        # self.logger.info("pid " + str(self.process_id) + " - " + magnet)
        self.logger.info("pid_{0} - {1}".format(self.process_id, magnet))

    def on_message(self, msg, address):
        """
        负责返回信息的处理

        :param msg: 报文信息
        :param address: 报文地址
        """
        try:
            if msg[b"y"] == b"r":
                if msg[b"r"].get(b"nodes", None):
                    self.on_find_node_response(msg)
            elif msg[b"y"] == b"q":
                if msg[b"q"] == b"get_peers":
                    self.on_get_peers_request(msg, address)
                elif msg[b"q"] == b"announce_peer":
                    self.on_announce_peer_request(msg, address)
        except KeyError:
            pass

    def on_find_node_response(self, msg):
        """
        解码 nodes 节点信息，并存储在双端队列

        :param msg: 节点报文信息
        """
        nodes = get_nodes_info(msg[b"r"][b"nodes"])
        for node in nodes:
            nid, ip, port = node
            if len(nid) != PER_NID_LEN or ip == self.bind_ip:
                continue
            self.nodes.append(HNode(nid, ip, port))

    def on_get_peers_request(self, msg, address):
        """
        处理 get_peers 请求，获取 info hash

        :param msg: 节点报文信息
        :param address: 节点地址
        """
        tid = msg[b"t"]
        try:
            info_hash = msg[b"a"][b"info_hash"]
            self.save_magnet(info_hash)
        except KeyError:
            self.send_error(tid, address)

    def on_announce_peer_request(self, msg, address):
        """
        处理 get_announce 请求，获取 info hash，address, port
        本爬虫目的暂时只是爬取磁链，所以忽略 address, port 有需要的
        开发者可自行完善这部分内容

        :param msg: 节点报文信息
        :param address: 节点地址
        """
        tid = msg[b"t"]
        try:
            info_hash = msg[b"a"][b"info_hash"]
            self.save_magnet(info_hash)
        except KeyError:
            # 没有对应的 info hash，发送错误回复
            self.send_error(tid, address)

    def receive_response_forever(self):
        """
        循环接受 udp 数据
        """
        self.logger.info(
            "receive response forever {}:{}".format(self.bind_ip, self.bind_port)
        )
        self.bootstrap()
        while True:
            try:
                data, address = self.udp.recvfrom(UDP_RECV_BUFFSIZE)
                msg = bencoder.bdecode(data)
                self.on_message(msg, address)
                time.sleep(SLEEP_TIME)
            except Exception as e:
                self.logger.warning(e)


def _start_thread(offset):
    """
    启动线程

    :param offset: 端口偏移值
    """
    dht = DHTServer(SERVER_HOST, SERVER_PORT + offset, offset)
    threads = [
        Thread(target=dht.send_find_node_forever),
        Thread(target=dht.receive_response_forever),
        Thread(target=dht.bs_timer),
    ]

    for t in threads:
        t.start()

    for t in threads:
        t.join()
