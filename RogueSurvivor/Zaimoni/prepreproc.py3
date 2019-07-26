#!/usr/bin/python
# designed for Python 3.7.0, may work with other versions
# (C)2019 Kenneth Boyd

# this implements a simplistic pre-preprocessor

from sys import argv;
from string import ascii_letters,digits;

command_start = '//* '	# 1-line comment start with atypical prefix
len_command_start = len(command_start)
copy_buffers = {}
for_loops = []

# we rely on $ not being in the source code character set in C-like languages
def var_replace(var,value,src):
	n = src.find(var)
	if -1>=n:
		return src
	skip = len(var)
	working = src
	ret = ''
	while -1<n:
		ret += working[:n]
		working = working[n:]
		test = working[skip:skip+1]
		if test and (test in ascii_letters or test in digits):
			ret += working[:skip]
		else:
			ret += value
		working = working[skip:]
		n = working.find(var)
	return ret

def sim_substitute(src,dest,lines):
	ret = []
	if src!=dest:
		for x in lines:
			ret.append(x.replace(src,dest))
	else:
		for x in lines:
			ret.append(x)
	return ret

def exec_substitute(in_substitute):
	global copy_buffers
	for x in sim_substitute(in_substitute[0],in_substitute[1],copy_buffers[in_substitute[2]]):
		print(x)

def sim_loop_substitute(in_substitute,loops):
	global copy_buffers
	if loops:
		ret = []
		var_name = '$'+loops[-1][0]
		for var_value in loops[-1][1]:
			src = var_replace(var_name,var_value,in_substitute[0])
			dest = var_replace(var_name,var_value,in_substitute[1])
			for x in sim_loop_substitute((src,dest,in_substitute[2]),loops[:-1]):
				ret.append(x)
		return ret
	else:
		return sim_substitute(in_substitute[0],in_substitute[1],copy_buffers[in_substitute[2]])

def exec_loop_substitute(in_substitute,loops):
	global copy_buffers
	working = sim_loop_substitute(in_substitute,loops)
	# handle the case where the first substitution of each loop is an identity as an exception
	leading_identity_map = 1
	n = len(in_substitute[2])
	while 0 < n:
		n -= 1
		if in_substitute[2][n]!=working[n]:
			leading_identity_map = 0
			break
	if leading_identity_map:
		working = working[len(in_substitute[2]):]
	for x in working:
		print(x)

if __name__ == "__main__":
	src = open(argv[1],'r')		# likely parameter
	# read in src; obtain all enum identifiers and prepare functions based on that
	in_copy = ''
	in_substitute = ()
	for line in src:
		# these two denotate a read-only variable (the "master" from which pre-preprocessing is done)
		if line.startswith(command_start+'start_copy '):
			if in_copy or in_substitute or for_loops:
				continue
			in_copy = line[len_command_start+11:].strip()
			print(line.rstrip())
			continue
		elif line.startswith(command_start+'end_copy'):
			if in_substitute or not in_copy:
				continue
			in_copy = ''
			print(line.rstrip())
			continue
		elif line.startswith(command_start+'for '):
			if in_copy or in_substitute:	# ok to nest for loops
				continue
			working = line[len_command_start+4:].strip()
			n = working.find(' in ')
			if -1>=n:
				continue
			for_var = working[:n].strip()
			for_prelist = working[n+4:].strip()
			if not for_prelist:
				continue
			print(line.rstrip())
			n = for_prelist.find(',')
			if -1>=n:
				for_loops.append((for_var,(for_prelist)))
			else:
				words = for_prelist.split(',')
				# \todo strip leading/trailing whitespace for safety
				for_loops.append((for_var,tuple(words)))
			continue
		elif line.startswith(command_start+'done'):
			if for_loops:
				print(line.rstrip())
				for_loops = for_loops[:-1]
			continue
		# this designates a block of text that is pre-preprocessed and thus is expected to be ovewritten
		elif line.startswith(command_start+'substitute '):
			# reserved keywords: for, in
			working = line[len_command_start+11:].strip()
			n = working.find(' for ')
			if -1>=n:
				continue
			target = working[:n].strip()
			working = working[n+5:].strip()
			n = working.find(' in ')
			if -1>=n:
				continue
			source = working[:n].strip()
			buffer = working[n+4:].strip()
			if buffer not in copy_buffers:
				continue
			in_substitute = (source,target,buffer)
			print(line.rstrip())
			continue
		elif line.startswith(command_start+'end_substitute'):
			if in_copy or not in_substitute:
				continue
			if for_loops:
				exec_loop_substitute(in_substitute,for_loops)
			else:
				exec_substitute(in_substitute)
			in_substitute = ()
			print(line.rstrip())
			continue
		elif in_copy:
			if in_copy in copy_buffers:
				copy_buffers[in_copy].append(line.rstrip())
			else:
				copy_buffers[in_copy] = [line.rstrip()]
		elif in_substitute:
			continue	# work done on ending
		print(line.rstrip())